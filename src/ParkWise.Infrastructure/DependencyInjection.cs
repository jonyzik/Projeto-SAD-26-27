using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParkWise.Application.Abstractions;
using ParkWise.Infrastructure.Persistence;

namespace ParkWise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddParkWiseInfrastructure(
        this IServiceCollection services, string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<ParkWiseDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IParkWiseDbContext>(provider => provider.GetRequiredService<ParkWiseDbContext>());
        }

        return services;
    }
}
