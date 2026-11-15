using TalentScout.Models.Enums;

namespace TalentScout.Models.Entities;

public sealed class CastingProject
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Client { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public DateOnly? ShootingDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ProjectRequirement? Requirement { get; set; }
    public ICollection<CastingApplication> CastingApplications { get; set; } = new List<CastingApplication>();

    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "Проєкт без назви" : Title;
    public string StatusLabel => Status switch
    {
        ProjectStatus.Draft => "Чернетка",
        ProjectStatus.Active => "Активний",
        ProjectStatus.Closed => "Закритий",
        _ => "Невідомо"
    };

    public override string ToString() => DisplayTitle;
}

public sealed class ProjectRequirement
{
    public int Id { get; set; }
    public int CastingProjectId { get; set; }
    public Gender? RequiredGender { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public int? MinimumHeightCm { get; set; }
    public int? MaximumHeightCm { get; set; }
    public EyeColor? RequiredEyeColor { get; set; }
    public HairColor? RequiredHairColor { get; set; }
    public int? CustomHairColorId { get; set; }
    public SkillMatchMode RequiredSkillsMatchMode { get; set; } = SkillMatchMode.All;
    public string? Notes { get; set; }
    public CastingProject CastingProject { get; set; } = null!;
    public ICollection<RequirementSkill> RequiredSkills { get; set; } = new List<RequirementSkill>();
    public HairColorOption? CustomHairColor { get; set; }
}

public sealed class RequirementSkill
{
    public int ProjectRequirementId { get; set; }
    public int SkillId { get; set; }
    public ProjectRequirement ProjectRequirement { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}

public sealed class CastingApplication
{
    public int Id { get; set; }
    public int CastingProjectId { get; set; }
    public int TalentId { get; set; }
    public CastingStatus Status { get; set; } = CastingStatus.Invited;
    public DateTime AddedUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public CastingProject CastingProject { get; set; } = null!;
    public Talent Talent { get; set; } = null!;

    public string StatusLabel => Status switch
    {
        CastingStatus.Invited => "Запрошений",
        CastingStatus.Audition => "Проби",
        CastingStatus.Approved => "Затверджений",
        CastingStatus.Reserve => "Резерв",
        CastingStatus.Rejected => "Відхилений",
        _ => "Невідомо"
    };

    public string NextStageLabel => Status switch
    {
        CastingStatus.Invited => "До проб →",
        CastingStatus.Audition => "Затвердити →",
        CastingStatus.Approved => "У резерв →",
        _ => "Етап завершено"
    };

    public bool CanMoveToNextStage => Status is CastingStatus.Invited or CastingStatus.Audition or CastingStatus.Approved;
}
