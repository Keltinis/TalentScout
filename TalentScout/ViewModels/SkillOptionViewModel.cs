using CommunityToolkit.Mvvm.ComponentModel;
using TalentScout.Models.Entities;

namespace TalentScout.ViewModels;

public sealed partial class SkillOptionViewModel : ObservableObject
{
    public SkillOptionViewModel(Skill skill) => Skill = skill;

    public Skill Skill { get; }
    public string Name => Skill.Name;

    [ObservableProperty]
    private bool isSelected;
}
