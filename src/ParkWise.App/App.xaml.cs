using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ParkWise.App.ViewModels;
using ParkWise.App.Navigation;
using ParkWise.Application.Dashboard;
using ParkWise.Infrastructure;
using ParkWise.Application.ScienceParks;
using ParkWise.App.Demo;

namespace ParkWise.App;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var registrations = new ServiceCollection();
        registrations.AddSingleton<IDashboardService, DashboardService>();
        registrations.AddSingleton<DashboardViewModel>();
        registrations.AddSingleton<IScienceParkCatalog, DemoScienceParkCatalog>();
        registrations.AddSingleton<ScienceParkSelection>();
        registrations.AddSingleton<ScienceParkPageState>();
        registrations.AddSingleton<ScienceParkSearchViewModel>();
        registrations.AddSingleton<ScienceParkDetailsViewModel>();
        registrations.AddSingleton<ScienceParkComparisonViewModel>();
        registrations.AddParkWisePages();
        registrations.AddSingleton<MainWindowViewModel>();
        registrations.AddSingleton<IPageNavigation>(provider => provider.GetRequiredService<MainWindowViewModel>());
        registrations.AddSingleton<MainWindow>();
        registrations.AddParkWiseInfrastructure(
            Environment.GetEnvironmentVariable("PARKWISE_SQLSERVER_CONNECTION"));

        _services = registrations.BuildServiceProvider();
        var window = _services.GetRequiredService<MainWindow>();
        window.Show();
        _services.GetRequiredService<MainWindowViewModel>().ActivateCurrentPage();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
