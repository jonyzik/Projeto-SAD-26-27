using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Simulations;

namespace ParkWise.Tests;

[TestClass]
public sealed class EigenvectorAhpEngineTests
{
    private readonly EigenvectorAhpEngine _engine = new();

    [TestMethod]
    public void EqualComparisonsProduceEqualWeightsAndZeroInconsistency()
    {
        var matrix = new double[4, 4];
        for (var i = 0; i < 4; i++) for (var j = 0; j < 4; j++) matrix[i, j] = 1;

        var result = _engine.CalculatePriorities(matrix);

        foreach (var weight in result.Weights) Assert.AreEqual(0.25, weight, 1e-9);
        Assert.AreEqual(4, result.LambdaMax, 1e-9);
        Assert.AreEqual(0, result.ConsistencyRatio, 1e-9);
    }

    [TestMethod]
    public void PerfectlyConsistentMatrixRecoversKnownWeights()
    {
        // Pesos 0,6 / 0,3 / 0,1 → a_ij = w_i / w_j.
        double[] w = [0.6, 0.3, 0.1];
        var matrix = new double[3, 3];
        for (var i = 0; i < 3; i++) for (var j = 0; j < 3; j++) matrix[i, j] = w[i] / w[j];

        var result = _engine.CalculatePriorities(matrix);

        for (var i = 0; i < 3; i++) Assert.AreEqual(w[i], result.Weights[i], 1e-9);
        Assert.AreEqual(0, result.ConsistencyRatio, 1e-9);
    }

    [TestMethod]
    public void ClassicSaatyExampleHasExpectedConsistencyRatio()
    {
        // Matriz 3×3 clássica: [1 3 5; 1/3 1 3; 1/5 1/3 1] → λmax ≈ 3,0385; RC ≈ 0,033.
        double[,] matrix = { { 1, 3, 5 }, { 1.0 / 3, 1, 3 }, { 1.0 / 5, 1.0 / 3, 1 } };

        var result = _engine.CalculatePriorities(matrix);

        Assert.AreEqual(0.637, result.Weights[0], 0.001);
        Assert.AreEqual(0.258, result.Weights[1], 0.001);
        Assert.AreEqual(0.105, result.Weights[2], 0.001);
        Assert.AreEqual(3.0385, result.LambdaMax, 0.001);
        Assert.AreEqual(0.033, result.ConsistencyRatio, 0.001);
    }

    [TestMethod]
    public void SynthesisIsTheWeightedSumOfLocalPriorities()
    {
        double[,] local = { { 0.5, 0.2 }, { 0.5, 0.8 } };

        var result = _engine.Synthesize([0.75, 0.25], local);

        Assert.AreEqual(0.425, result[0], 1e-12);
        Assert.AreEqual(0.575, result[1], 1e-12);
    }

    [TestMethod]
    public void RejectsNonPositiveValues()
    {
        double[,] matrix = { { 1, 0 }, { 1, 1 } };
        Assert.ThrowsExactly<ArgumentException>(() => _engine.CalculatePriorities(matrix));
    }
}
