using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TalentScout.Models.Entities;
using TalentScout.Models.Enums;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Interfaces;

namespace TalentScout.ViewModels;

public sealed partial class CastingBoardViewModel(IProjectRepository projectRepository, ICastingService castingService) : ViewModelBase
{
    public ObservableCollection<CastingProject> Projects { get; } = [];
    public ObservableCollection<CastingApplication> Candidates { get; } = [];
    [ObservableProperty] private CastingProject? selectedProject;

    partial void OnSelectedProjectChanged(CastingProject? value) => _ = LoadCandidatesAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        Projects.Clear();
        foreach (var project in await projectRepository.GetAllAsync()) Projects.Add(project);
        SelectedProject ??= Projects.FirstOrDefault();
    }

    [RelayCommand]
    private async Task MoveNextAsync(CastingApplication application)
    {
        if (SelectedProject is null) return;
        var next = application.Status switch { CastingStatus.Invited => CastingStatus.Audition, CastingStatus.Audition => CastingStatus.Approved, CastingStatus.Approved => CastingStatus.Reserve, _ => CastingStatus.Reserve };
        await castingService.ChangeStatusAsync(SelectedProject.Id, application.TalentId, next);
        await LoadCandidatesAsync();
    }

    private async Task LoadCandidatesAsync()
    {
        Candidates.Clear();
        if (SelectedProject is null) return;
        foreach (var candidate in await castingService.GetCandidatesAsync(SelectedProject.Id)) Candidates.Add(candidate);
    }
}
