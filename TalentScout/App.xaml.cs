using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TalentScout.Data;
using TalentScout.Repositories.Implementations;
using TalentScout.Repositories.Interfaces;
using TalentScout.Services.Implementations;
using TalentScout.Services.Interfaces;

namespace TalentScout;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        var services = new ServiceCollection();
        var dataDirectory = Path.Combine(GetStorageRoot(), "Data");
        Directory.CreateDirectory(dataDirectory);
        var databasePath = Path.Combine(dataDirectory, "TalentScout.db");

        services.AddDbContextFactory<TalentScoutDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddSingleton<DatabaseInitializer>();
        services.AddTransient<ITalentRepository, TalentRepository>();
        services.AddTransient<ISkillRepository, SkillRepository>();
        services.AddTransient<IHairColorOptionRepository, HairColorOptionRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<ITalentSearchService, TalentSearchService>();
        services.AddSingleton<IPhotoStorageService, PhotoStorageService>();
        services.AddSingleton<IFilePickerService, FilePickerService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<ICastingService, CastingService>();
        services.AddTransient<ITalentProfileService, TalentProfileService>();
        services.AddTransient<ViewModels.TalentsViewModel>();
        services.AddTransient<ViewModels.SearchViewModel>();
        services.AddTransient<ViewModels.DashboardViewModel>();
        services.AddTransient<ViewModels.ProjectsViewModel>();
        services.AddTransient<ViewModels.CastingBoardViewModel>();

        Services = services.BuildServiceProvider();
    }

    private static string GetStorageRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TalentScout.sln")))
            {
                return directory.FullName;
            }
        }

        return AppContext.BaseDirectory;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            Services.GetRequiredService<DatabaseInitializer>().InitializeAsync().GetAwaiter().GetResult();
            base.OnStartup(e);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"Не вдалося підготувати локальну базу даних.\n\n{exception.Message}",
                "TalentScout",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}

