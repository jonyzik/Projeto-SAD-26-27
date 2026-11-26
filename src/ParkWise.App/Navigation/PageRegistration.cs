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
            "Resumo e atalhos.",
            "\uE80F",
            () => provider.GetRequiredService<DashboardViewModel>(),
            () => provider.GetRequiredService<DashboardViewModel>().RefreshCommand.Execute(null)));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.ScienceParks,
            "Parques de Ciência",
            "Pesquisar parques",
            "Procure por nome ou localização.",
            "\uE80D",
            () => provider.GetRequiredService<ScienceParkSearchViewModel>(),
            () => provider.GetRequiredService<ScienceParkSearchViewModel>().RefreshCommand.Execute(null)));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.ScienceParkDetails,
            "Detalhes do parque",
            "Detalhes do parque",
            "Informação principal e critérios.",
            "\uE80D",
            () => provider.GetRequiredService<ScienceParkDetailsViewModel>(),
            () => _ = provider.GetRequiredService<ScienceParkDetailsViewModel>().LoadAsync(),
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.ScienceParks));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.ScienceParkComparison,
            "Comparação preliminar",
            "Comparação preliminar",
            "Reveja as alternativas escolhidas.",
            "\uE80D",
            () => provider.GetRequiredService<ScienceParkComparisonViewModel>(),
            () => _ = provider.GetRequiredService<ScienceParkComparisonViewModel>().LoadAsync(),
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.ScienceParks));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.Simulations,
            "Simulações",
            "Simulações",
            "Defina as preferências da decisão.",
            "\uE7C3",
            () => provider.GetRequiredService<AhpCriteriaConfigurationViewModel>(),
            () => _ = provider.GetRequiredService<AhpCriteriaConfigurationViewModel>().LoadAsync()));

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.History,
            "Histórico",
            "Histórico",
            "Análises anteriores.",
            "\uE81C",
            () => new PlaceholderViewModel(
                "Histórico de simulações",
                "A consulta de simulações anteriores está em desenvolvimento.",
                ["Lista de simulações realizadas", "Consulta dos resultados guardados"])));

        services.AddSingleton<IPageRoute>(_ => new PageRoute(
            PageKeys.Settings,
            "Definições",
            "Definições",
            "Preferências da aplicação.",
            "\uE713",
            () => new PlaceholderViewModel(
                "Definições",
                "As definições da aplicação estão em desenvolvimento.",
                ["Preferências da aplicação", "Gestão de conta e permissões"])));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.NewSimulation,
            "Configurar critérios AHP",
            "Simulações",
            "Defina as preferências da decisão.",
            "\uE7C3",
            () => provider.GetRequiredService<AhpCriteriaConfigurationViewModel>(),
            () => _ = provider.GetRequiredService<AhpCriteriaConfigurationViewModel>().LoadAsync(),
            IsVisibleInSidebar: false,
            ParentMenuKey: PageKeys.Simulations));

        services.AddSingleton<IPageRoute>(provider => new PageRoute(
            PageKeys.Login,
            "Login",
            "Iniciar sessão",
            "Aceda à sua conta.",
            "\uE77B",
            () => provider.GetRequiredService<LoginViewModel>(),
            IsVisibleInSidebar: false));

        return services;
    }
}
