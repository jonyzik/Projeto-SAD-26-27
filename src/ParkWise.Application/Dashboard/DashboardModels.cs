namespace ParkWise.Application.Dashboard;

public sealed record DashboardPark(
    Guid Id,
    string Name,
    string City,
    decimal? MonthlyRentPerSquareMetre,
    DateTime CreatedAtUtc,
    int EvaluatedCriteriaCount);

public sealed record DashboardSimulation(Guid Id, string Name, DateTime CreatedAtUtc);

public sealed record DashboardData(
    IReadOnlyList<DashboardPark> Parks,
    IReadOnlyList<DashboardSimulation> Simulations,
    bool IsDemonstration);

public sealed record PriceBand(string Label, int Count);

public sealed record DashboardSummary(
    int TotalParks,
    int TotalSimulations,
    int ComparableParks,
    decimal? AveragePricePerSquareMetre,
    IReadOnlyList<DashboardPark> RecentParks,
    IReadOnlyList<DashboardSimulation> RecentSimulations,
    IReadOnlyList<PriceBand> PriceDistribution,
    bool IsDemonstration)
{
    public bool HasPriceDistribution => PriceDistribution.Sum(band => band.Count) >= 2;
}
