using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ParkWise.Application.Abstractions;
using ParkWise.Application.Ahp;
using ParkWise.Application.Dashboard;
using ParkWise.Application.Simulations;
using ParkWise.Infrastructure.Dashboard;
using ParkWise.Infrastructure.Identity;
using ParkWise.Infrastructure.Persistence;
using ParkWise.Infrastructure.Simulations;

namespace ParkWise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddParkWiseInfrastructure(
        this IServiceCollection services, string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContextFactory<ParkWiseDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IParkWiseDbContext>(provider => provider.GetRequiredService<ParkWiseDbContext>());
            services.AddSingleton<IDashboardDataSource, EfDashboardDataSource>();
            services.AddSingleton<ISimulationDataSource, EfSimulationDataSource>();
        }
        else
        {
            services.AddSingleton<IDashboardDataSource, DemoDashboardDataSource>();
            services.AddSingleton<ISimulationDataSource, DemoSimulationDataSource>();
        }

        // Simulações: o motor AHP de referência pode ser substituído pelo do módulo dev/ahp
        // registando outra implementação de IAhpEngine antes desta chamada.
        services.TryAddSingleton<IAhpEngine, EigenvectorAhpEngine>();
        services.TryAddSingleton<ICurrentUserAccessor, AnonymousCurrentUserAccessor>();
        services.AddSingleton<ISimulationParametersStore>(_ => new JsonSimulationParametersStore());
        services.AddSingleton<ISimulationService, SimulationService>();

        return services;
    }
}
