using TalentScout.Models.Enums;

namespace TalentScout.ViewModels;

public sealed record SelectionOption<T>(T Value, string Title)
{
    public override string ToString() => Title;
}

public sealed record HairColorChoice(HairColor? BuiltInColor, int? CustomColorId, string Title)
{
    public override string ToString() => Title;
}
