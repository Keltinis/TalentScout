using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TalentScout.ViewModels;

namespace TalentScout.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        var viewModel = ((App)Application.Current).Services.GetRequiredService<DashboardViewModel>();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadCommand.ExecuteAsync(null);
    }

    private void OpenTalents_Click(object sender, RoutedEventArgs e) => ((MainWindow)Application.Current.MainWindow).OpenTalents();
}
