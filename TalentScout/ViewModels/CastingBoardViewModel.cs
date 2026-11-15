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
    [ObservableProperty] private string statusMessage = string.Empty;

    partial void OnSelectedProjectChanged(CastingProject? value) => _ = LoadCandidatesAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        Projects.Clear();
        foreach (var project in await projectRepository.GetAllAsync()) Projects.Add(project);
        SelectedProject ??= Projects.FirstOrDefault();
    }

    [RelayCommand]
    private async Task MoveNextAsync(CastingApplication? application)
    {
        if (application is null || !application.CanMoveToNextStage) return;

        try
        {
            var next = application.Status switch
            {
                CastingStatus.Invited => CastingStatus.Audition,
                CastingStatus.Audition => CastingStatus.Approved,
                CastingStatus.Approved => CastingStatus.Reserve,
                _ => application.Status
            };
            await castingService.ChangeStatusAsync(application.Id, next);
            StatusMessage = $"Для «{application.Talent.FullName}» встановлено етап: {GetStatusTitle(next)}.";
            // Read a fresh list from SQLite: the change is tied to the exact CastingApplication.Id
            // from the pressed row, and all labels/button states are redrawn reliably.
            await LoadCandidatesAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = $"Не вдалося змінити етап: {exception.Message}";
        }
    }

    private async Task LoadCandidatesAsync()
    {
        Candidates.Clear();
        if (SelectedProject is null) return;
        foreach (var candidate in await castingService.GetCandidatesAsync(SelectedProject.Id)) Candidates.Add(candidate);
    }

    private static string GetStatusTitle(CastingStatus status) => status switch
    {
        CastingStatus.Invited => "Запрошений",
        CastingStatus.Audition => "Проби",
        CastingStatus.Approved => "Затверджений",
        CastingStatus.Reserve => "Резерв",
        CastingStatus.Rejected => "Відхилений",
        _ => "Невідомо"
    };
}
