using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Dashboard;

namespace ParkWise.Tests;

[TestClass]
public sealed class DashboardServiceTests
{
    [TestMethod]
    public async Task CalculatesIndicatorsRecentItemsAndPriceBandsWithoutWpf()
    {
        var oldest = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var parks = new[]
        {
            Park("A", 8.50m, 9, oldest),
            Park("B", 14m, 9, oldest.AddDays(2)),
            Park("C", 22m, 7, oldest.AddDays(1)),
            Park("D", null, 9, oldest.AddDays(3)),
            Park("E", 0m, 9, oldest.AddDays(4)),
            Park("F", 35m, 9, oldest.AddDays(5))
        };
        var simulations = new[]
        {
            new DashboardSimulation(Guid.NewGuid(), "Anterior", oldest),
            new DashboardSimulation(Guid.NewGuid(), "Recente", oldest.AddDays(3))
        };
        var service = new DashboardService(new StubSource(new DashboardData(parks, simulations, true)));

        var result = await service.GetSummaryAsync();

        Assert.AreEqual(6, result.TotalParks);
        Assert.AreEqual(2, result.TotalSimulations);
        Assert.AreEqual(3, result.ComparableParks);
        Assert.AreEqual(19.88m, result.AveragePricePerSquareMetre);
        CollectionAssert.AreEqual(new[] { 1, 1, 1, 1 }, result.PriceDistribution.Select(x => x.Count).ToArray());
        Assert.IsTrue(result.HasPriceDistribution);
        Assert.IsTrue(result.IsDemonstration);
        CollectionAssert.AreEqual(new[] { "F", "E", "D", "B", "C" }, result.RecentParks.Select(x => x.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "Recente", "Anterior" }, result.RecentSimulations.Select(x => x.Name).ToArray());
    }

    [TestMethod]
    public async Task EmptySourceHasNoAverageOrChart()
    {
        var service = new DashboardService(new StubSource(new DashboardData(
            Array.Empty<DashboardPark>(), Array.Empty<DashboardSimulation>(), false)));

        var result = await service.GetSummaryAsync();

        Assert.AreEqual(0, result.TotalParks);
        Assert.AreEqual(0, result.TotalSimulations);
        Assert.AreEqual(0, result.ComparableParks);
        Assert.IsNull(result.AveragePricePerSquareMetre);
        Assert.IsFalse(result.HasPriceDistribution);
        Assert.IsFalse(result.IsDemonstration);
    }

    [TestMethod]
    public async Task PriceIntervalsIncludeTheirLowerBoundWithoutOverlap()
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var parks = new[]
        {
            Park("Dez", 10m, 9, date),
            Park("Vinte", 20m, 9, date),
            Park("Trinta", 30m, 9, date),
            Park("Inválido", -5m, 9, date)
        };
        var service = new DashboardService(new StubSource(new DashboardData(
            parks, Array.Empty<DashboardSimulation>(), false)));

        var result = await service.GetSummaryAsync();

        CollectionAssert.AreEqual(new[] { 0, 1, 1, 1 }, result.PriceDistribution.Select(x => x.Count).ToArray());
        Assert.AreEqual(20m, result.AveragePricePerSquareMetre);
        Assert.AreEqual(3, result.ComparableParks);
    }

    private static DashboardPark Park(string name, decimal? price, int criteria, DateTime createdAt) =>
        new(Guid.NewGuid(), name, "Localidade", price, createdAt, criteria);

    private sealed class StubSource(DashboardData data) : IDashboardDataSource
    {
        public Task<DashboardData> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(data);
    }
}
