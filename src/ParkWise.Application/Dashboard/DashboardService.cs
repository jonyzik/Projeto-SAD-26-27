using ParkWise.Core;

namespace ParkWise.Application.Dashboard;

public sealed class DashboardService(IDashboardDataSource dataSource) : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var data = await dataSource.LoadAsync(cancellationToken);
        var pricedParks = data.Parks
            .Where(park => park.MonthlyRentPerSquareMetre is > 0)
            .ToList();

        var priceDistribution = new[]
        {
            new PriceBand("< 10 €", pricedParks.Count(park => park.MonthlyRentPerSquareMetre < 10)),
            new PriceBand("10–19,99 €", pricedParks.Count(park => park.MonthlyRentPerSquareMetre is >= 10 and < 20)),
            new PriceBand("20–29,99 €", pricedParks.Count(park => park.MonthlyRentPerSquareMetre is >= 20 and < 30)),
            new PriceBand("≥ 30 €", pricedParks.Count(park => park.MonthlyRentPerSquareMetre >= 30))
        };

        return new DashboardSummary(
            TotalParks: data.Parks.Count,
            TotalSimulations: data.Simulations.Count,
            ComparableParks: pricedParks.Count(park => park.EvaluatedCriteriaCount >= EvaluationCriteria.Required.Count),
            AveragePricePerSquareMetre: pricedParks.Count == 0
                ? null
                : Math.Round(pricedParks.Average(park => park.MonthlyRentPerSquareMetre!.Value), 2, MidpointRounding.AwayFromZero),
            RecentParks: data.Parks.OrderByDescending(park => park.CreatedAtUtc).Take(5).ToArray(),
            RecentSimulations: data.Simulations.OrderByDescending(simulation => simulation.CreatedAtUtc).Take(5).ToArray(),
            PriceDistribution: priceDistribution,
            IsDemonstration: data.IsDemonstration);
    }
}
