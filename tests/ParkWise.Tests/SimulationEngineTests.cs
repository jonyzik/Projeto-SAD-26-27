using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Simulations;
using ParkWise.Core.Enums;
using static ParkWise.Tests.SimulationTestData;

namespace ParkWise.Tests;

[TestClass]
public sealed class SimulationEngineTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private readonly SimulationEngine _engine = new(new EigenvectorAhpEngine());

    [TestMethod]
    public void WithoutComparisonsAllCriteriaWeighTheSame()
    {
        var preview = _engine.CalculateWeights([]);

        Assert.AreEqual(9, preview.Weights.Count);
        foreach (var weight in preview.Weights) Assert.AreEqual(1m / 9m, weight.Weight, 0.000001m);
        Assert.IsTrue(preview.Consistency.IsAcceptable);
    }

    [TestMethod]
    public void ComparisonsChangeTheWeightsInTheExpectedDirection()
    {
        var preview = _engine.CalculateWeights(
        [
            new CriterionComparison(CriterionKind.Wifi, CriterionKind.LeisureArea, 9m),
            new CriterionComparison(CriterionKind.Wifi, CriterionKind.Parking, 3m)
        ]);

        var wifi = preview.Weights.Single(w => w.Kind == CriterionKind.Wifi).Weight;
        var leisure = preview.Weights.Single(w => w.Kind == CriterionKind.LeisureArea).Weight;
        Assert.IsTrue(wifi > leisure);
        Assert.AreEqual(1m, preview.Weights.Sum(w => w.Weight), 0.00001m);
    }

    [TestMethod]
    public void CheaperParkWinsWhenEverythingElseIsEqual()
    {
        var cheap = Park("Barato", 8m);
        var expensive = Park("Caro", 20m);

        var outcome = _engine.Run(Request([expensive, cheap]), [cheap, expensive], Now);

        Assert.AreEqual("Barato", outcome.BestResult!.ParkName);
        Assert.AreEqual(1, outcome.Results.Single(r => r.ParkName == "Barato").Ranking);
        Assert.AreEqual(2, outcome.Results.Single(r => r.ParkName == "Caro").Ranking);
        Assert.AreEqual(1m, outcome.RankedResults.Sum(r => r.Score), 0.0001m);
        CollectionAssert.Contains(outcome.BestResult.Strengths.ToList(), "Preço competitivo da área alugada, por m²");
        Assert.AreEqual(Now, outcome.ExecutedAtUtc);
    }

    [TestMethod]
    public void PreferencesCanOutweighTheCost()
    {
        var cheapNoWifi = Park("Sem Wi-Fi", 8m, overrides: new Dictionary<CriterionKind, decimal> { [CriterionKind.Wifi] = 1m });
        var expensiveWifi = Park("Com Wi-Fi", 11m, overrides: new Dictionary<CriterionKind, decimal> { [CriterionKind.Wifi] = 10m });
        var comparisons = SimulationTestData.Criteria
            .Where(kind => kind != CriterionKind.Wifi)
            .Select(kind => new CriterionComparison(CriterionKind.Wifi, kind, 9m))
            .ToArray();

        var outcome = _engine.Run(Request([cheapNoWifi, expensiveWifi], comparisons), [cheapNoWifi, expensiveWifi], Now);

        Assert.AreEqual("Com Wi-Fi", outcome.BestResult!.ParkName);
        CollectionAssert.Contains(outcome.BestResult.Strengths.ToList(), "Rede Wi-Fi");
        CollectionAssert.Contains(outcome.Results.Single(r => r.ParkName == "Sem Wi-Fi").Weaknesses.ToList(), "Rede Wi-Fi");
    }

    [TestMethod]
    public void ExcludesParksThatDoNotMeetTheCompanyNeeds()
    {
        var ok = Park("Elegível", 10m);
        var noPrice = Park("Sem preço", null);
        var incomplete = Park("Incompleto", 10m, omitWifi: true);
        var noParking = Park("Sem estacionamento", 10m, overrides: new Dictionary<CriterionKind, decimal> { [CriterionKind.Parking] = 0m });
        var otherCity = Park("Lisboa", 10m, city: "Lisboa");
        var tooExpensive = Park("Demasiado caro", 60m);
        var parks = new[] { ok, noPrice, incomplete, noParking, otherCity, tooExpensive };

        var parameters = new SimulationParameters(
            new CompanyNeeds(100m, 5, 12, MaxMonthlyBudget: 3000m, PreferredCity: "porto"),
            CostParameters.Default);
        var outcome = _engine.Run(Request(parks, parameters: parameters, mandatory: [CriterionKind.Parking]), parks, Now);

        Assert.AreEqual(1, outcome.RankedResults.Count);
        Assert.AreEqual("Elegível", outcome.BestResult!.ParkName);
        Assert.AreEqual(1m, outcome.BestResult.Score, 0.000001m);
        Assert.AreEqual(0, outcome.BestResult.Strengths.Count);

        string Reason(string name) => string.Join(" ", outcome.Results.Single(r => r.ParkName == name).ExclusionReasons);
        StringAssert.Contains(Reason("Sem preço"), "preço");
        StringAssert.Contains(Reason("Incompleto"), "Rede Wi-Fi");
        StringAssert.Contains(Reason("Sem estacionamento"), "Parque de estacionamento");
        StringAssert.Contains(Reason("Lisboa"), "Localização");
        StringAssert.Contains(Reason("Demasiado caro"), "orçamento");
        Assert.IsTrue(outcome.Results.Where(r => !r.IsEligible).All(r => r.Ranking is null));
    }

    [TestMethod]
    public void InconsistentComparisonsAreRejected()
    {
        var a = Park("A", 10m);
        var b = Park("B", 12m);
        CriterionComparison[] cyclic =
        [
            new(CriterionKind.Wifi, CriterionKind.Parking, 9m),
            new(CriterionKind.Parking, CriterionKind.LeisureArea, 9m),
            new(CriterionKind.LeisureArea, CriterionKind.Wifi, 9m)
        ];

        var error = Assert.ThrowsExactly<SimulationValidationException>(() => _engine.Run(Request([a, b], cyclic), [a, b], Now));
        StringAssert.Contains(error.Errors[0], "inconsistentes");
    }

    [TestMethod]
    public void InvalidRequestsReturnAllValidationErrors()
    {
        var a = Park("A", 10m);
        var parameters = new SimulationParameters(new CompanyNeeds(0m, 0, 0, -1m, null), CostParameters.Default with { ParkingSpacesRatio = 2m });

        var error = Assert.ThrowsExactly<SimulationValidationException>(() =>
            _engine.Run(Request([a], parameters: parameters, name: " "), [a], Now));

        Assert.IsTrue(error.Errors.Count >= 6);
    }

    [TestMethod]
    public void UnknownParksAreReported()
    {
        var a = Park("A", 10m);
        var b = Park("B", 10m);

        var error = Assert.ThrowsExactly<SimulationValidationException>(() => _engine.Run(Request([a, b]), [a], Now));
        StringAssert.Contains(error.Errors.Single(), "já não existem");
    }
}
