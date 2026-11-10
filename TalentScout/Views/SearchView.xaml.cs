using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TalentScout.ViewModels;

namespace TalentScout.Views;

public partial class SearchView : UserControl
{
    private readonly SearchViewModel _viewModel;

    public SearchView()
    {
        InitializeComponent();
        _viewModel = ((App)Application.Current).Services.GetRequiredService<SearchViewModel>();
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
