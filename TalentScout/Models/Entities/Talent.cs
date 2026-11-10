using TalentScout.Models.Enums;

namespace TalentScout.Models.Entities;

public sealed class Talent
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public int? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public EyeColor EyeColor { get; set; }
    public HairColor HairColor { get; set; }
    public int? CustomHairColorId { get; set; }
    public string? ClothingSize { get; set; }
    public string? ShoeSize { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TalentPhoto> Photos { get; set; } = new List<TalentPhoto>();
    public ICollection<TalentSkill> TalentSkills { get; set; } = new List<TalentSkill>();
    public ICollection<CastingApplication> CastingApplications { get; set; } = new List<CastingApplication>();
    public HairColorOption? CustomHairColor { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class HairColorOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Talent> Talents { get; set; } = new List<Talent>();
    public ICollection<ProjectRequirement> ProjectRequirements { get; set; } = new List<ProjectRequirement>();

    public override string ToString() => Name;
}

public sealed class TalentPhoto
{
    public int Id { get; set; }
    public int TalentId { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public Talent Talent { get; set; } = null!;
}
