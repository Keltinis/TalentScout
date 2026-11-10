namespace TalentScout.ViewModels;

public sealed record SelectionOption<T>(T Value, string Title)
{
    public override string ToString() => Title;
}
