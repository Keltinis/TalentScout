using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.ViewModels;

public sealed partial class DashboardViewModel(ITalentRepository talentRepository, IProjectRepository projectRepository) : ViewModelBase
{
    [ObservableProperty] private int talentsCount;
    [ObservableProperty] private int activeProjectsCount;
    [ObservableProperty] private int candidatesCount;

    [RelayCommand]
    public async Task LoadAsync()
    {
        var talents = await talentRepository.GetAllAsync();
        var projects = await projectRepository.GetAllAsync();
        TalentsCount = talents.Count;
        ActiveProjectsCount = projects.Count(x => x.Status == ProjectStatus.Active);
        CandidatesCount = projects.Sum(x => x.CastingApplications.Count);
    }
}
