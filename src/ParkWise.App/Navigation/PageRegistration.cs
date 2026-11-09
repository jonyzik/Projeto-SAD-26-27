using Microsoft.Extensions.DependencyInjection;
using ParkWise.App.ViewModels;
using ParkWise.Application.Simulations;

namespace ParkWise.App.Navigation;

public static class PageRegistration
{
    public static IServiceCollection AddParkWisePages(this IServiceCollection services)
    {
        services.AddSingleton<SimulationSession>();

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.Dashboard,
            "Dashboard",
            "Dashboard",
            "Apoio à escolha do parque de ciência mais adequado para a sua empresa.",
            "\uE80F",
            () => provider.GetRequiredService<DashboardViewModel>(),
            () => provider.GetRequiredService<DashboardViewModel>().RefreshCommand.Execute(null)));

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.ScienceParks,
            "Parques de Ciência",
            "Parques de Ciência e Tecnologia",
            "Explore alternativas para a instalação da sua empresa.",
            "\uE80D",
            () => new PlaceholderViewModel(
                "Parques de Ciência e Tecnologia",
                "O catálogo e a pesquisa de parques estão em desenvolvimento.",
                ["Pesquisa por localização e serviços", "Consulta dos nove critérios obrigatórios", "Seleção de parques para comparação"])));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.Simulations,
            "Simulações",
            "Simulações",
            "Crie, execute e consulte simulações com as preferências da sua empresa.",
            "\uE7C3",
            () =>
            {
                var viewModel = new SimulationsViewModel(
                    provider.GetRequiredService<ISimulationService>(),
                    provider.GetRequiredService<SimulationSession>(),
                    key => Navigate(provider, key),
                    Dialogs.Confirm);
                viewModel.LoadCommand.Execute(null);
                return viewModel;
            }));

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.History,
            "Histórico",
            "Histórico",
            "Reveja as análises realizadas quando este módulo estiver disponível.",
            "\uE81C",
            () => new PlaceholderViewModel(
                "Histórico de simulações",
                "A consulta de simulações anteriores está em desenvolvimento.",
                ["Lista de simulações realizadas", "Consulta dos resultados guardados"])));

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.Settings,
            "Definições",
            "Definições",
            "Configure a aplicação e as suas preferências.",
            "\uE713",
            () => new PlaceholderViewModel(
                "Definições",
                "As definições da aplicação estão em desenvolvimento.",
                ["Preferências da aplicação", "Gestão de conta e permissões"])));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.NewSimulation,
            "Nova simulação",
            "Nova simulação",
            "Defina as necessidades da empresa, compare os critérios pelo método AHP e calcule os custos operacionais.",
            "\uE7C3",
            () =>
            {
                var viewModel = new NewSimulationViewModel(
                    provider.GetRequiredService<ISimulationService>(),
                    provider.GetRequiredService<SimulationSession>(),
                    key => Navigate(provider, key));
                viewModel.InitializeCommand.Execute(null);
                return viewModel;
            },
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.Simulations));

        return services;
    }

    // Resolvido apenas no momento da navegação, para evitar dependências circulares com a MainWindowViewModel.
    private static void Navigate(IServiceProvider provider, string key) =>
        provider.GetRequiredService<MainWindowViewModel>().NavigateCommand.Execute(key);
}
