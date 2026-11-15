using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;

namespace TalentScout.ViewModels;

public sealed partial class DashboardViewModel(ITalentRepository talentRepository, IProjectRepository projectRepository) : ViewModelBase
{
    public ObservableCollection<Talent> RecentTalents { get; } = [];
    public ObservableCollection<CastingProject> UpcomingProjects { get; } = [];
    [ObservableProperty] private int talentsCount;
    [ObservableProperty] private int activeProjectsCount;
    [ObservableProperty] private int candidatesCount;
    [ObservableProperty] private string talentsCountText = "0";
    [ObservableProperty] private string activeProjectsCountText = "0";
    [ObservableProperty] private string candidatesCountText = "0";

    [RelayCommand]
    public async Task LoadAsync()
    {
        var talents = await talentRepository.GetAllAsync();
        var projects = await projectRepository.GetAllAsync();
        TalentsCount = talents.Count;
        ActiveProjectsCount = projects.Count(x => x.Status == ProjectStatus.Active);
        CandidatesCount = projects.Sum(x => x.CastingApplications.Count);
        TalentsCountText = TalentsCount.ToString();
        ActiveProjectsCountText = ActiveProjectsCount.ToString();
        CandidatesCountText = CandidatesCount.ToString();
        RecentTalents.Clear();
        foreach (var talent in talents.OrderByDescending(x => x.CreatedUtc).Take(5)) RecentTalents.Add(talent);
        UpcomingProjects.Clear();
        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var project in projects.Where(x => x.ShootingDate is not null && x.ShootingDate >= today).OrderBy(x => x.ShootingDate).Take(5)) UpcomingProjects.Add(project);
    }
}
