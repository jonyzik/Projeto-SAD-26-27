using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.ScienceParks;
using ParkWise.Core;
using ParkWise.Core.AHP;
using ParkWise.Core.Enums;

namespace ParkWise.Tests;

[TestClass]
public sealed class ScienceParkRankingServiceTests
{
    private readonly ScienceParkRankingService _service = new();

    [TestMethod]
    public void RanksCompleteParksUsingAhpWeightsAndExplainsEachContribution()
    {
        var parks = new[]
        {
            Park("Parque de Lisboa", "Lisboa", 10m),
            Park("Parque do Porto", "Porto", 20m)
        };

        var result = _service.Rank(parks, EqualWeights(), "Lisboa");

        Assert.AreEqual(2, result.RankedParks.Count);
        Assert.AreEqual("Parque de Lisboa", result.RankedParks[0].Park.Name);
        Assert.AreEqual(1, result.RankedParks[0].Rank);
        Assert.AreEqual(1.0, result.RankedParks[0].Score, 1e-10);
        Assert.AreEqual("Parque do Porto", result.RankedParks[1].Park.Name);
        Assert.AreEqual(0.8, result.RankedParks[1].Score, 1e-10);
        Assert.AreEqual(10, result.RankedParks[0].Criteria.Count);
        Assert.AreEqual(
            CriterionKind.GeographicLocation,
            result.RankedParks[0].Criteria[0].Kind);
        Assert.AreEqual(0.1, result.RankedParks[0].Criteria[0].Contribution, 1e-10);
        Assert.AreEqual(0.1, result.RankedParks[0].Criteria[9].Contribution, 1e-10);
        Assert.AreEqual(0.0, result.RankedParks[1].Criteria[9].Contribution, 1e-10);
    }

    [TestMethod]
    public void ParksWithMissingCriteriaAreExcludedAndExplained()
    {
        var incompletePark = Park("Parque sem preço", "Coimbra", null);
        var result = _service.Rank(
            [Park("Parque A", "Lisboa", 10m), Park("Parque B", "Porto", 12m), incompletePark],
            EqualWeights(),
            "Lisboa");

        Assert.AreEqual(2, result.RankedParks.Count);
        Assert.AreEqual(1, result.UnrankedParks.Count);
        Assert.AreSame(incompletePark, result.UnrankedParks[0].Park);
        StringAssert.Contains(result.UnrankedParks[0].Reason, "Preço competitivo");
    }

    [TestMethod]
    public void ScoresUnavailableServicesAsZeroAndHandlesEqualRents()
    {
        var result = _service.Rank(
            [
                Park("Parque completo", "Lisboa", 10m),
                Park("Parque sem Wi-Fi", "Lisboa", 10m, CriterionKind.Wifi)
            ],
            EqualWeights(),
            "Lisboa");

        Assert.AreEqual(1.0, result.RankedParks[0].Score, 1e-10);
        Assert.AreEqual(0.9, result.RankedParks[1].Score, 1e-10);
        var wifi = result.RankedParks[1].Criteria
            .Single(criterion => criterion.Kind == CriterionKind.Wifi);
        Assert.AreEqual(0.0, wifi.Score, 1e-10);
        Assert.AreEqual(0.0, wifi.Contribution, 1e-10);
    }

    [TestMethod]
    public void ParksWithEqualScoresShareTheSameRank()
    {
        var result = _service.Rank(
            [Park("Parque A", "Lisboa", 10m), Park("Parque B", "Lisboa", 10m)],
            EqualWeights(),
            "Lisboa");

        Assert.AreEqual(1, result.RankedParks[0].Rank);
        Assert.AreEqual(1, result.RankedParks[1].Rank);
        Assert.AreEqual(result.RankedParks[0].Score, result.RankedParks[1].Score, 1e-10);
    }

    [TestMethod]
    public void DoesNotRankWhenFewerThanTwoParksHaveCompleteData()
    {
        var result = _service.Rank(
            [Park("Parque A", "Lisboa", 10m), Park("Parque B", "Porto", null)],
            EqualWeights(),
            "Lisboa");

        Assert.AreEqual(0, result.RankedParks.Count);
        Assert.AreEqual(2, result.UnrankedParks.Count);
        Assert.IsTrue(result.UnrankedParks.Any(entry =>
            entry.Reason.Contains("pelo menos duas alternativas", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void RejectsWeightsThatDoNotCoverEveryAhpCriterion()
    {
        var weights = EqualWeights().Where(pair => pair.Key != CriterionKind.GeographicLocation)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        Assert.Throws<ArgumentException>(() =>
            _service.Rank([Park("Parque A", "Lisboa", 10m), Park("Parque B", "Porto", 12m)], weights, "Lisboa"));
    }

    private static IReadOnlyDictionary<CriterionKind, double> EqualWeights() =>
        AHPCriteria.Required.ToDictionary(criterion => criterion.Kind, _ => 0.1);

    private static SciencePark Park(
        string name,
        string city,
        decimal? rent,
        params CriterionKind[] unavailable) =>
        new(
            Guid.NewGuid(),
            name,
            city,
            string.Empty,
            rent,
            EvaluationCriteria.Required
                .Where(criterion => criterion.Kind != CriterionKind.CompetitiveRentPerSquareMetre)
                .ToDictionary(criterion => criterion.Kind, criterion => !unavailable.Contains(criterion.Kind)));
}
