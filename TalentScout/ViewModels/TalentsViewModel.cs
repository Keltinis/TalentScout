using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Interfaces;

namespace TalentScout.ViewModels;

public sealed partial class TalentsViewModel : ViewModelBase
{
    private readonly ITalentRepository _talentRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IHairColorOptionRepository _hairColorRepository;
    private readonly ITalentProfileService _profileService;
    private readonly IFilePickerService _filePickerService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Talent> Talents { get; } = [];
    public ObservableCollection<SkillOptionViewModel> SkillOptions { get; } = [];
    public ObservableCollection<HairColorOption> CustomHairColors { get; } = [];
    public IReadOnlyList<SelectionOption<Gender>> Genders { get; } =
    [new(Gender.Female, "Жінка"), new(Gender.Male, "Чоловік"), new(Gender.Other, "Інше")];
    public IReadOnlyList<SelectionOption<HairColor>> HairColors { get; } =
    [new(HairColor.Blonde, "Блонд"), new(HairColor.Brunette, "Каштанове"), new(HairColor.Black, "Чорне"), new(HairColor.Red, "Руде"), new(HairColor.Gray, "Сиве")];

    [ObservableProperty] private string firstName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private string heightCm = string.Empty;
    [ObservableProperty] private DateTime birthDate = DateTime.Today.AddYears(-22);
    [ObservableProperty] private SelectionOption<Gender>? selectedGender;
    [ObservableProperty] private SelectionOption<HairColor>? selectedHairColor;
    [ObservableProperty] private HairColorOption? selectedCustomHairColor;
    [ObservableProperty] private string? primaryPhotoPath;
    [ObservableProperty] private string newHairColorName = string.Empty;
    [ObservableProperty] private string newSkillName = string.Empty;
    [ObservableProperty] private string statusMessage = "Заповніть форму праворуч, щоб додати новий профіль.";
    public string PhotoLabel => string.IsNullOrWhiteSpace(PrimaryPhotoPath) ? "Фото ще не вибрано" : Path.GetFileName(PrimaryPhotoPath);

    public TalentsViewModel(
        ITalentRepository talentRepository,
        ISkillRepository skillRepository,
        IHairColorOptionRepository hairColorRepository,
        ITalentProfileService profileService,
        IFilePickerService filePickerService,
        IDialogService dialogService)
    {
        _talentRepository = talentRepository;
        _skillRepository = skillRepository;
        _hairColorRepository = hairColorRepository;
        _profileService = profileService;
        _filePickerService = filePickerService;
        _dialogService = dialogService;
        SelectedGender = Genders[0];
        SelectedHairColor = HairColors[0];
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Talents.Clear();
        foreach (var talent in await _talentRepository.GetAllAsync()) Talents.Add(talent);
        SkillOptions.Clear();
        foreach (var skill in await _skillRepository.GetAllAsync()) SkillOptions.Add(new SkillOptionViewModel(skill));
        CustomHairColors.Clear();
        CustomHairColors.Add(new HairColorOption { Id = 0, Name = "Власний колір (не обрано)" });
        foreach (var color in await _hairColorRepository.GetAllAsync()) CustomHairColors.Add(color);
        SelectedCustomHairColor ??= CustomHairColors[0];
    }

    [RelayCommand]
    private void ChoosePhoto()
    {
        var path = _filePickerService.PickImageFile();
        if (path is not null)
        {
            PrimaryPhotoPath = path;
            StatusMessage = $"Вибрано фото: {Path.GetFileName(path)}";
        }
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
            StatusMessage = "Новий колір додано до довідника.";
        }
        catch (Exception exception) { StatusMessage = exception.Message; }
    }

    [RelayCommand]
    private async Task AddSkillAsync()
    {
        try
        {
            await _skillRepository.AddAsync(new Skill { Name = NewSkillName, Category = SkillCategory.Other });
            NewSkillName = string.Empty;
            await LoadAsync();
            StatusMessage = "Нову навичку додано до довідника.";
        }
        catch (Exception exception) { StatusMessage = exception.Message; }
    }

    [RelayCommand]
    private async Task SaveTalentAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || !int.TryParse(HeightCm, out var height))
        {
            StatusMessage = "Вкажіть ім’я, прізвище та коректний зріст.";
            return;
        }

        try
        {
            var talent = new Talent
            {
                FirstName = FirstName.Trim(), LastName = LastName.Trim(), City = City.Trim(), HeightCm = height,
                BirthDate = DateOnly.FromDateTime(BirthDate), Gender = SelectedGender?.Value ?? Gender.Unspecified, HairColor = SelectedHairColor?.Value ?? HairColor.Unspecified,
                CustomHairColorId = SelectedCustomHairColor?.Id > 0 ? SelectedCustomHairColor.Id : null,
                TalentSkills = SkillOptions.Where(x => x.IsSelected).Select(x => new TalentSkill { SkillId = x.Skill.Id, Level = SkillLevel.Intermediate }).ToList()
            };
            await _profileService.CreateAsync(talent, PrimaryPhotoPath ?? string.Empty);
            StatusMessage = $"Анкету «{talent.FullName}» успішно створено.";
            FirstName = LastName = City = HeightCm = string.Empty;
            PrimaryPhotoPath = null;
            foreach (var skill in SkillOptions) skill.IsSelected = false;
            await LoadAsync();
        }
        catch (Exception exception) { StatusMessage = exception.Message; }
    }

    [RelayCommand]
    private async Task DeleteTalentAsync(Talent talent)
    {
        if (!_dialogService.Confirm($"Видалити анкету «{talent.FullName}»? Цю дію не можна скасувати.", "Підтвердження видалення")) return;
        await _talentRepository.DeleteAsync(talent.Id);
        StatusMessage = $"Анкету «{talent.FullName}» видалено.";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task EditTalentAsync(Talent talent)
    {
        var details = await _talentRepository.GetDetailsAsync(talent.Id);
        if (details is null) return;
        var window = new Views.TalentEditorWindow(details) { Owner = Application.Current.MainWindow };
        if (window.ShowDialog() == true) await LoadAsync();
    }

    partial void OnPrimaryPhotoPathChanged(string? value) => OnPropertyChanged(nameof(PhotoLabel));
}
