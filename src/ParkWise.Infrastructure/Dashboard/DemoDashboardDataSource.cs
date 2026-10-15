using ParkWise.Application.Dashboard;

namespace ParkWise.Infrastructure.Dashboard;

public sealed class DemoDashboardDataSource : IDashboardDataSource
{
    public Task<DashboardData> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTime.UtcNow;

        DashboardPark[] parks =
        [
            new(Guid.Parse("10000000-0000-0000-0000-000000000001"), "Parque Fictício A", "Localidade fictícia", 8.50m, now.AddDays(-1), 9),
            new(Guid.Parse("10000000-0000-0000-0000-000000000002"), "Parque Fictício B", "Localidade fictícia", 14.00m, now.AddDays(-3), 9),
            new(Guid.Parse("10000000-0000-0000-0000-000000000003"), "Parque Fictício C", "Localidade fictícia", 22.00m, now.AddDays(-8), 7),
            new(Guid.Parse("10000000-0000-0000-0000-000000000004"), "Parque Fictício D", "Localidade fictícia", null, now.AddDays(-12), 0)
        ];

        DashboardSimulation[] simulations =
        [
            new(Guid.Parse("20000000-0000-0000-0000-000000000001"), "Simulação fictícia A", now.AddDays(-2)),
            new(Guid.Parse("20000000-0000-0000-0000-000000000002"), "Simulação fictícia B", now.AddDays(-5))
        ];

        return Task.FromResult(new DashboardData(parks, simulations, IsDemonstration: true));
    }
}
