using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalentScout.Models.Dtos;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Interfaces;

namespace TalentScout.ViewModels;

public sealed partial class SearchViewModel : ViewModelBase
{
    private readonly ITalentSearchService _searchService;
    private readonly ISkillRepository _skillRepository;
    private readonly IHairColorOptionRepository _hairColorRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICastingService _castingService;

    public ObservableCollection<Talent> Results { get; } = [];
    public ObservableCollection<SkillOptionViewModel> SkillOptions { get; } = [];
    public ObservableCollection<HairColorChoice> HairColorChoices { get; } = [];
    public ObservableCollection<CastingProject> Projects { get; } = [];
    public IReadOnlyList<SelectionOption<Gender?>> Genders { get; } =
    [new(null, "Будь-яка стать"), new(Gender.Female, "Жінка"), new(Gender.Male, "Чоловік"), new(Gender.Other, "Інше")];
    private static readonly IReadOnlyList<HairColorChoice> StandardHairColors =
    [new(null, null, "Будь-яке волосся"), new(HairColor.Blonde, null, "Блонд"), new(HairColor.Brunette, null, "Каштанове"), new(HairColor.Black, null, "Чорне"), new(HairColor.Red, null, "Руде"), new(HairColor.Gray, null, "Сиве")];

    [ObservableProperty] private SelectionOption<Gender?>? selectedGender;
    [ObservableProperty] private HairColorChoice? selectedHairColor;
    [ObservableProperty] private string minimumAge = "20";
    [ObservableProperty] private string maximumAge = "25";
    [ObservableProperty] private string minimumHeight = "170";
    [ObservableProperty] private string maximumHeight = "190";
    [ObservableProperty] private string newHairColorName = string.Empty;
    [ObservableProperty] private string newSkillName = string.Empty;
    [ObservableProperty] private string resultCaption = "Налаштуйте критерії та натисніть «Знайти збіги».";
    [ObservableProperty] private CastingProject? selectedProject;

    partial void OnSelectedProjectChanged(CastingProject? value) => _ = RefreshProjectMembershipAsync();

    public SearchViewModel(ITalentSearchService searchService, ISkillRepository skillRepository, IHairColorOptionRepository hairColorRepository, IProjectRepository projectRepository, ICastingService castingService)
    {
        _searchService = searchService;
        _skillRepository = skillRepository;
        _hairColorRepository = hairColorRepository;
        _projectRepository = projectRepository;
        _castingService = castingService;
        SelectedGender = Genders[0];
        SelectedHairColor = StandardHairColors[0];
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        SkillOptions.Clear();
        foreach (var skill in await _skillRepository.GetAllAsync()) SkillOptions.Add(new SkillOptionViewModel(skill));
        var previouslySelected = SelectedHairColor?.Title;
        HairColorChoices.Clear();
        foreach (var color in StandardHairColors) HairColorChoices.Add(color);
        foreach (var color in await _hairColorRepository.GetAllAsync()) HairColorChoices.Add(new HairColorChoice(null, color.Id, color.Name));
        SelectedHairColor = HairColorChoices.FirstOrDefault(x => x.Title == previouslySelected) ?? HairColorChoices.FirstOrDefault();
        Projects.Clear();
        foreach (var project in await _projectRepository.GetAllAsync()) Projects.Add(project);
        SelectedProject ??= Projects.FirstOrDefault();
    }

    [RelayCommand]
    private async Task AddHairColorAsync()
    {
        try
        {
            var id = await _hairColorRepository.AddAsync(NewHairColorName);
            NewHairColorName = string.Empty;
            await LoadAsync();
            SelectedHairColor = HairColorChoices.Single(x => x.CustomColorId == id);
        }
        catch (Exception exception) { ResultCaption = exception.Message; }
    }

    [RelayCommand]
    private async Task AddSkillAsync()
    {
        try
        {
            await _skillRepository.AddAsync(new Skill { Name = NewSkillName, Category = SkillCategory.Other });
            NewSkillName = string.Empty;
            await LoadAsync();
        }
        catch (Exception exception) { ResultCaption = exception.Message; }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        var criteria = new TalentSearchCriteria
        {
            Gender = SelectedGender?.Value,
            HairColor = SelectedHairColor?.BuiltInColor,
            CustomHairColorId = SelectedHairColor?.CustomColorId,
            MinimumAge = ParseNumber(MinimumAge), MaximumAge = ParseNumber(MaximumAge),
            MinimumHeightCm = ParseNumber(MinimumHeight), MaximumHeightCm = ParseNumber(MaximumHeight),
            SkillIds = SkillOptions.Where(x => x.IsSelected).Select(x => x.Skill.Id).ToArray(),
            SkillMatchMode = SkillMatchMode.All
        };
        Results.Clear();
        foreach (var talent in await _searchService.SearchAsync(criteria)) Results.Add(talent);
        await RefreshProjectMembershipAsync();
        ResultCaption = $"Знайдено {Results.Count} талантів за заданими критеріями.";
    }

    [RelayCommand]
    private async Task AddCandidateAsync(Talent talent)
    {
        if (SelectedProject is null) { ResultCaption = "Спочатку оберіть проєкт."; return; }
        if (!talent.CanBeAddedToSelectedProject)
        {
            ResultCaption = $"{talent.FullName} уже доданий до «{SelectedProject.Title}».";
            return;
        }
        try
        {
            await _castingService.AddCandidateAsync(SelectedProject.Id, talent.Id);
            // Reload the displayed search result set from SQLite so the pressed row
            // immediately receives the disabled "У проєкті" state.
            await SearchAsync();
            ResultCaption = $"{talent.FullName} додано до «{SelectedProject.Title}».";
        }
        catch (Exception exception)
        {
            ResultCaption = exception.Message;
            await RefreshProjectMembershipAsync();
        }
    }

    private async Task RefreshProjectMembershipAsync()
    {
        var addedTalentIds = SelectedProject is null
            ? new HashSet<int>()
            : (await _castingService.GetCandidatesAsync(SelectedProject.Id)).Select(x => x.TalentId).ToHashSet();

        foreach (var talent in Results)
        {
            talent.CanBeAddedToSelectedProject = !addedTalentIds.Contains(talent.Id);
        }

        // Replace collection items so the button styles redraw after a project is switched.
        for (var index = 0; index < Results.Count; index++) Results[index] = Results[index];
    }

    private static int? ParseNumber(string value) => int.TryParse(value, out var number) ? number : null;
}
