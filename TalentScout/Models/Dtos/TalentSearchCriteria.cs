using TalentScout.Models.Enums;

namespace TalentScout.Models.Dtos;

public sealed class TalentSearchCriteria
{
    public string? Text { get; init; }
    public Gender? Gender { get; init; }
    public int? MinimumAge { get; init; }
    public int? MaximumAge { get; init; }
    public int? MinimumHeightCm { get; init; }
    public int? MaximumHeightCm { get; init; }
    public EyeColor? EyeColor { get; init; }
    public HairColor? HairColor { get; init; }
    public int? CustomHairColorId { get; init; }
    public IReadOnlyCollection<int> SkillIds { get; init; } = Array.Empty<int>();
    public SkillMatchMode SkillMatchMode { get; init; } = SkillMatchMode.All;
}
