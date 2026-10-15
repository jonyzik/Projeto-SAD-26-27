using Microsoft.EntityFrameworkCore;
using ParkWise.Application.Dashboard;
using ParkWise.Infrastructure.Persistence;

namespace ParkWise.Infrastructure.Dashboard;

public sealed class EfDashboardDataSource(IDbContextFactory<ParkWiseDbContext> contextFactory)
    : IDashboardDataSource
{
    public async Task<DashboardData> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var parks = await context.TechnologyParks.AsNoTracking()
            .Select(park => new
            {
                park.Id,
                park.Name,
                park.City,
                park.MonthlyRentPerSquareMetre,
                park.CreatedAtUtc,
                EvaluatedCriteriaCount = park.Evaluations
                    .Select(evaluation => evaluation.EvaluationCriterionId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync(cancellationToken);

        var simulations = await context.Simulations.AsNoTracking()
            .Select(simulation => new { simulation.Id, simulation.Name, simulation.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        return new DashboardData(
            parks.Select(park => new DashboardPark(
                park.Id, park.Name, park.City, park.MonthlyRentPerSquareMetre,
                park.CreatedAtUtc, park.EvaluatedCriteriaCount)).ToArray(),
            simulations.Select(simulation => new DashboardSimulation(
                simulation.Id, simulation.Name, simulation.CreatedAtUtc)).ToArray(),
            IsDemonstration: false);
    }
}
