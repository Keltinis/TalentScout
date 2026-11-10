using System.Windows; using System.Windows.Controls; using Microsoft.Extensions.DependencyInjection; using TalentScout.ViewModels;
namespace TalentScout.Views;
public partial class CastingBoardView : UserControl { public CastingBoardView(){ InitializeComponent(); var vm=((App)Application.Current).Services.GetRequiredService<CastingBoardViewModel>(); DataContext=vm; Loaded += async (_,_) => await vm.LoadCommand.ExecuteAsync(null); } }
