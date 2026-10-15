using Microsoft.Extensions.DependencyInjection;
using ParkWise.App.ViewModels;

namespace ParkWise.App.Navigation;

public static class PageRegistration
{
    public static IServiceCollection AddParkWisePages(this IServiceCollection services)
    {
        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.Dashboard,
            "Dashboard",
            "Dashboard",
            "Apoio à escolha do parque de ciência mais adequado para a sua empresa.",
            "\uE80F",
            () => provider.GetRequiredService<DashboardViewModel>(),
            () => provider.GetRequiredService<DashboardViewModel>().RefreshCommand.Execute(null)));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.ScienceParks,
            "Parques de Ciência",
            "Parques de Ciência e Tecnologia",
            "Explore alternativas para a instalação da sua empresa.",
            "\uE80D",
            () => provider.GetRequiredService<ScienceParkSearchViewModel>(),
            () => provider.GetRequiredService<ScienceParkSearchViewModel>().RefreshCommand.Execute(null)));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.ScienceParkDetails,
            "Detalhes do parque",
            "Detalhes do parque",
            "Consulte os nove critérios obrigatórios e selecione o parque para comparar.",
            "\uE80D",
            () => provider.GetRequiredService<ScienceParkDetailsViewModel>(),
            () => _ = provider.GetRequiredService<ScienceParkDetailsViewModel>().LoadAsync(),
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.ScienceParks));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.ScienceParkComparison,
            "Comparação preliminar",
            "Comparação preliminar",
            "Veja os parques selecionados lado a lado antes da futura simulação AHP.",
            "\uE80D",
            () => provider.GetRequiredService<ScienceParkComparisonViewModel>(),
            () => _ = provider.GetRequiredService<ScienceParkComparisonViewModel>().LoadAsync(),
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.ScienceParks));

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.Simulations,
            "Simulações",
            "Simulações",
            "Compare alternativas com as preferências da sua empresa.",
            "\uE7C3",
            () => new PlaceholderViewModel(
                "Simulações",
                "A criação e o cálculo de simulações pelo método AHP estão em desenvolvimento.",
                ["Definição de preferências", "Comparações pelos nove critérios", "Resultados e custos operacionais"])));

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

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.NewSimulation,
            "Nova simulação",
            "Nova simulação",
            "Prepare uma nova análise de parques de ciência e tecnologia.",
            "\uE7C3",
            () => new PlaceholderViewModel(
                "Nova simulação",
                "A criação de uma nova simulação está em desenvolvimento.",
                ["Seleção de parques", "Preferências pelos nove critérios", "Análise AHP e custos"]),
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.Simulations));

        return services;
    }
}
