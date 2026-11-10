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
    public ObservableCollection<HairColorOption> CustomHairColors { get; } = [];
    public ObservableCollection<CastingProject> Projects { get; } = [];
    public IReadOnlyList<SelectionOption<Gender?>> Genders { get; } =
    [new(null, "Будь-яка стать"), new(Gender.Female, "Жінка"), new(Gender.Male, "Чоловік"), new(Gender.Other, "Інше")];
    public IReadOnlyList<SelectionOption<HairColor?>> HairColors { get; } =
    [new(null, "Будь-яке волосся"), new(HairColor.Blonde, "Блонд"), new(HairColor.Brunette, "Каштанове"), new(HairColor.Black, "Чорне"), new(HairColor.Red, "Руде"), new(HairColor.Gray, "Сиве")];

    [ObservableProperty] private SelectionOption<Gender?>? selectedGender;
    [ObservableProperty] private SelectionOption<HairColor?>? selectedHairColor;
    [ObservableProperty] private HairColorOption? selectedCustomHairColor;
    [ObservableProperty] private string minimumAge = "20";
    [ObservableProperty] private string maximumAge = "25";
    [ObservableProperty] private string minimumHeight = "170";
    [ObservableProperty] private string maximumHeight = "190";
    [ObservableProperty] private string newHairColorName = string.Empty;
    [ObservableProperty] private string newSkillName = string.Empty;
    [ObservableProperty] private string resultCaption = "Налаштуйте критерії та натисніть «Знайти збіги».";
    [ObservableProperty] private CastingProject? selectedProject;

    public SearchViewModel(ITalentSearchService searchService, ISkillRepository skillRepository, IHairColorOptionRepository hairColorRepository, IProjectRepository projectRepository, ICastingService castingService)
    {
        _searchService = searchService;
        _skillRepository = skillRepository;
        _hairColorRepository = hairColorRepository;
        _projectRepository = projectRepository;
        _castingService = castingService;
        SelectedGender = Genders[0];
        SelectedHairColor = HairColors[0];
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        SkillOptions.Clear();
        foreach (var skill in await _skillRepository.GetAllAsync()) SkillOptions.Add(new SkillOptionViewModel(skill));
        CustomHairColors.Clear();
        CustomHairColors.Add(new HairColorOption { Id = 0, Name = "Власний колір (не обрано)" });
        foreach (var color in await _hairColorRepository.GetAllAsync()) CustomHairColors.Add(color);
        SelectedCustomHairColor ??= CustomHairColors[0];
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
            SelectedCustomHairColor = CustomHairColors.Single(x => x.Id == id);
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
            HairColor = SelectedCustomHairColor?.Id > 0 ? null : SelectedHairColor?.Value,
            CustomHairColorId = SelectedCustomHairColor?.Id > 0 ? SelectedCustomHairColor.Id : null,
            MinimumAge = ParseNumber(MinimumAge), MaximumAge = ParseNumber(MaximumAge),
            MinimumHeightCm = ParseNumber(MinimumHeight), MaximumHeightCm = ParseNumber(MaximumHeight),
            SkillIds = SkillOptions.Where(x => x.IsSelected).Select(x => x.Skill.Id).ToArray(),
            SkillMatchMode = SkillMatchMode.All
        };
        Results.Clear();
        foreach (var talent in await _searchService.SearchAsync(criteria)) Results.Add(talent);
        ResultCaption = $"Знайдено {Results.Count} талантів за заданими критеріями.";
    }

    [RelayCommand]
    private async Task AddCandidateAsync(Talent talent)
    {
        if (SelectedProject is null) { ResultCaption = "Спочатку оберіть проєкт."; return; }
        try { await _castingService.AddCandidateAsync(SelectedProject.Id, talent.Id); ResultCaption = $"{talent.FullName} додано до «{SelectedProject.Title}»."; }
        catch (Exception exception) { ResultCaption = exception.Message; }
    }

    private static int? ParseNumber(string value) => int.TryParse(value, out var number) ? number : null;
}
