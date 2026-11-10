using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TalentScout.ViewModels;

namespace TalentScout.Views;

public partial class TalentsView : UserControl
{
    private readonly TalentsViewModel _viewModel;

    public TalentsView()
    {
        InitializeComponent();
        _viewModel = ((App)Application.Current).Services.GetRequiredService<TalentsViewModel>();
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
