using System.Windows;
using TalentScout.Services.Interfaces;

namespace TalentScout.Services.Implementations;

public sealed class DialogService : IDialogService
{
    public bool Confirm(string message, string title) => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
}
