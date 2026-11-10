using System.Windows; using System.Windows.Controls; using Microsoft.Extensions.DependencyInjection; using TalentScout.ViewModels;
namespace TalentScout.Views;
public partial class ProjectsView : UserControl { public ProjectsView(){ InitializeComponent(); var vm=((App)Application.Current).Services.GetRequiredService<ProjectsViewModel>(); DataContext=vm; Loaded += async (_,_) => await vm.LoadCommand.ExecuteAsync(null); } }
