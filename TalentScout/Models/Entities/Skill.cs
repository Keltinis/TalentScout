using TalentScout.Models.Enums;

namespace TalentScout.Models.Entities;

public sealed class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public ICollection<TalentSkill> TalentSkills { get; set; } = new List<TalentSkill>();
    public ICollection<RequirementSkill> RequirementSkills { get; set; } = new List<RequirementSkill>();
}

public sealed class TalentSkill
{
    public int TalentId { get; set; }
    public int SkillId { get; set; }
    public SkillLevel Level { get; set; }
    public string? Notes { get; set; }
    public Talent Talent { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
