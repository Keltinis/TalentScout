using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.ViewModels;

public sealed partial class ProjectsViewModel(IProjectRepository projectRepository) : ViewModelBase
{
    public ObservableCollection<CastingProject> Projects { get; } = [];
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string client = string.Empty;
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string minimumAge = string.Empty;
    [ObservableProperty] private string maximumAge = string.Empty;
    [ObservableProperty] private string minimumHeight = string.Empty;
    [ObservableProperty] private SelectionOption<Gender?>? requiredGender;
    [ObservableProperty] private SelectionOption<HairColor?>? requiredHairColor;
    [ObservableProperty] private string statusMessage = string.Empty;
    public IReadOnlyList<SelectionOption<Gender?>> Genders { get; } =
    [new(null, "Будь-яка стать"), new(Gender.Female, "Жінка"), new(Gender.Male, "Чоловік"), new(Gender.Other, "Інше")];
    public IReadOnlyList<SelectionOption<HairColor?>> HairColors { get; } =
    [new(null, "Будь-яке волосся"), new(HairColor.Blonde, "Блонд"), new(HairColor.Brunette, "Каштанове"), new(HairColor.Black, "Чорне"), new(HairColor.Red, "Руде")];

    [RelayCommand]
    public async Task LoadAsync()
    {
        Projects.Clear();
        foreach (var project in await projectRepository.GetAllAsync()) Projects.Add(project);
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Title)) { StatusMessage = "Вкажіть назву проєкту."; return; }
        await projectRepository.AddAsync(new CastingProject
        {
            Title = Title.Trim(), Client = Client.Trim(), City = City.Trim(), Description = Description.Trim(), Status = ProjectStatus.Active,
            Requirement = new ProjectRequirement
            {
                RequiredGender = RequiredGender?.Value, RequiredHairColor = RequiredHairColor?.Value,
                MinimumAge = ParseNumber(MinimumAge), MaximumAge = ParseNumber(MaximumAge), MinimumHeightCm = ParseNumber(MinimumHeight)
            }
        });
        Title = Client = City = Description = string.Empty;
        StatusMessage = "Проєкт створено.";
        await LoadAsync();
    }

    private static int? ParseNumber(string value) => int.TryParse(value, out var number) ? number : null;
}
