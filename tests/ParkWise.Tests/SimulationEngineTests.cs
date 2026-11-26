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
    public void WithDefaultImportanceAllCriteriaWeighTheSame()
    {
        var preview = _engine.CalculateWeights(new Dictionary<CriterionKind, int>());

        Assert.AreEqual(9, preview.Weights.Count);
        foreach (var weight in preview.Weights)
        {
            Assert.AreEqual(1m / 9m, weight.Weight, 0.000001m);
            Assert.AreEqual(ImportanceScale.Default, weight.Importance);
        }
        Assert.IsTrue(preview.Consistency.IsAcceptable);
    }

    [TestMethod]
    public void WeightsAreProportionalToTheImportance()
    {
        var importance = SimulationTestData.Criteria.ToDictionary(kind => kind, _ => 3);
        importance[CriterionKind.Wifi] = 9;
        importance[CriterionKind.LeisureArea] = 1;

        var preview = _engine.CalculateWeights(importance);

        decimal W(CriterionKind kind) => preview.Weights.Single(w => w.Kind == kind).Weight;
        // Soma das notas = 9 + 1 + 7 × 3 = 31, por isso Wi-Fi = 9/31, lazer = 1/31 e os restantes 3/31.
        Assert.AreEqual(9m / 31m, W(CriterionKind.Wifi), 0.00001m);
        Assert.AreEqual(1m / 31m, W(CriterionKind.LeisureArea), 0.00001m);
        Assert.AreEqual(3m / 31m, W(CriterionKind.Parking), 0.00001m);
        Assert.AreEqual(1m, preview.Weights.Sum(w => w.Weight), 0.00001m);
        Assert.AreEqual(0m, preview.Consistency.ConsistencyRatio, 0.0001m);
    }

    [TestMethod]
    public void ImportanceIsConvertedIntoAReciprocalAhpMatrix()
    {
        var importance = new Dictionary<CriterionKind, int> { [CriterionKind.Wifi] = 9, [CriterionKind.Parking] = 3 };

        var matrix = PairwiseComparisonMatrix.FromImportance(SimulationTestData.Criteria, importance);

        var wifi = Array.IndexOf(SimulationTestData.Criteria, CriterionKind.Wifi);
        var parking = Array.IndexOf(SimulationTestData.Criteria, CriterionKind.Parking);
        Assert.AreEqual(3.0, matrix[wifi, parking], 1e-12);
        Assert.AreEqual(1.0 / 3.0, matrix[parking, wifi], 1e-12);
        Assert.AreEqual(1.0, matrix[wifi, wifi], 1e-12);
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
        var importance = SimulationTestData.Criteria.ToDictionary(kind => kind, _ => 1);
        importance[CriterionKind.Wifi] = 9;

        var outcome = _engine.Run(Request([cheapNoWifi, expensiveWifi], importance), [cheapNoWifi, expensiveWifi], Now);

        Assert.AreEqual("Com Wi-Fi", outcome.BestResult!.ParkName);
        CollectionAssert.Contains(outcome.BestResult.Strengths.ToList(), "Rede Wi-Fi");
        CollectionAssert.Contains(outcome.Results.Single(r => r.ParkName == "Sem Wi-Fi").Weaknesses.ToList(), "Rede Wi-Fi");
    }

    [TestMethod]
    public void ExcludesParksWithoutPriceCompleteEvaluationOrMandatoryCriteria()
    {
        var ok = Park("Elegível", 10m);
        var noPrice = Park("Sem preço", null);
        var incomplete = Park("Incompleto", 10m, omitWifi: true);
        var noParking = Park("Sem estacionamento", 10m, overrides: new Dictionary<CriterionKind, decimal> { [CriterionKind.Parking] = 0m });
        var parks = new[] { ok, noPrice, incomplete, noParking };

        var outcome = _engine.Run(Request(parks, mandatory: [CriterionKind.Parking]), parks, Now);

        Assert.AreEqual(1, outcome.RankedResults.Count);
        Assert.AreEqual("Elegível", outcome.BestResult!.ParkName);
        Assert.AreEqual(1m, outcome.BestResult.Score, 0.000001m);
        Assert.AreEqual(0, outcome.BestResult.Strengths.Count);

        string Reason(string name) => string.Join(" ", outcome.Results.Single(r => r.ParkName == name).ExclusionReasons);
        StringAssert.Contains(Reason("Sem preço"), "preço");
        StringAssert.Contains(Reason("Incompleto"), "Rede Wi-Fi");
        StringAssert.Contains(Reason("Sem estacionamento"), "Parque de estacionamento");
        Assert.IsTrue(outcome.Results.Where(r => !r.IsEligible).All(r => r.Ranking is null));
    }

    [TestMethod]
    public void ImportanceOutsideTheScaleIsRejected()
    {
        var a = Park("A", 10m);
        var b = Park("B", 12m);
        var importance = new Dictionary<CriterionKind, int> { [CriterionKind.Wifi] = 10, [CriterionKind.Parking] = 0 };

        var error = Assert.ThrowsExactly<SimulationValidationException>(() => _engine.Run(Request([a, b], importance), [a, b], Now));
        StringAssert.Contains(error.Errors.Single(), "entre 1 e 9");
    }

    [TestMethod]
    public void SavedOutcomeKeepsTheImportanceForReopening()
    {
        var a = Park("A", 10m);
        var b = Park("B", 12m);
        var importance = new Dictionary<CriterionKind, int> { [CriterionKind.Wifi] = 8 };

        var outcome = _engine.Run(Request([a, b], importance), [a, b], Now);

        Assert.AreEqual(8, outcome.CriterionImportance[CriterionKind.Wifi]);
        Assert.AreEqual(ImportanceScale.Default, outcome.CriterionImportance[CriterionKind.Parking]);
        Assert.AreEqual(8, outcome.ToRequest().CriterionImportance[CriterionKind.Wifi]);
    }

    [TestMethod]
    public void InvalidRequestsReturnAllValidationErrors()
    {
        var a = Park("A", 10m);
        var error = Assert.ThrowsExactly<SimulationValidationException>(() =>
            _engine.Run(Request([a], needs: new CompanyNeeds(0m, 0), name: " "), [a], Now));

        // Nome vazio, área 0, contrato de 0 meses e só um parque.
        Assert.AreEqual(4, error.Errors.Count);
    }

    [TestMethod]
    public void OperationalCostsUseTheAreaAndTheContractDuration()
    {
        var a = Park("A", 10m);
        var b = Park("B", 12.5m);

        var outcome = _engine.Run(Request([a, b], needs: new CompanyNeeds(80m, 24)), [a, b], Now);

        var costs = outcome.Results.Single(r => r.ParkName == "B").Costs!;
        Assert.AreEqual(1000m, costs.Monthly);
        Assert.AreEqual(12000m, costs.Annual);
        Assert.AreEqual(24000m, costs.ContractTotal);
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
