using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Core.AHP;

namespace ParkWise.Tests;

[TestClass]
public sealed class AhpCalculatorTests
{
    [TestMethod]
    public void CalculatesTenEqualCriteria()
    {
        var matrix = new AHPComparisonMatrix(10);

        for (var row = 0; row < matrix.Size; row++)
        {
            for (var column = row + 1; column < matrix.Size; column++)
                matrix.SetComparison(row, column, 1);
        }

        var result = new AhpCalculator().Calculate(matrix);

        Assert.AreEqual(10, result.Weights.Count);
        Assert.AreEqual(1.49, result.RandomIndex, 0.000001);
        Assert.AreEqual(0, result.ConsistencyRatio, 0.000001);
        foreach (var weight in result.Weights)
            Assert.AreEqual(0.1, weight, 0.000001);
    }

    [TestMethod]
    public void RejectsNullMatrixBeforeCheckingCompleteness()
    {
        Assert.Throws<ArgumentNullException>(
            () => new AhpCalculator().Calculate(null!));
    }

    [TestMethod]
    public void CalculatesNormalizedGeometricMeanWeights()
    {
        var matrix = new AHPComparisonMatrix(3);
        matrix.SetComparison(0, 1, 2);
        matrix.SetComparison(0, 2, 4);
        matrix.SetComparison(1, 2, 2);

        var result = new AhpCalculator().Calculate(matrix);

        Assert.AreEqual(0.5714285714, result.Weights[0], 0.000001);
        Assert.AreEqual(0.2857142857, result.Weights[1], 0.000001);
        Assert.AreEqual(0.1428571429, result.Weights[2], 0.000001);
        Assert.AreEqual(1, result.Weights.Sum(), 0.000001);
        Assert.AreEqual(3, result.LambdaMax, 0.000001);
        Assert.IsTrue(result.IsConsistent);
    }

    [TestMethod]
    public void FlagsAReciprocalMatrixWithInconsistentJudgments()
    {
        var matrix = new AHPComparisonMatrix(3);
        matrix.SetComparison(0, 1, 9);
        matrix.SetComparison(1, 2, 9);
        matrix.SetComparison(0, 2, 1d / 9d);

        var result = new AhpCalculator().Calculate(matrix);

        Assert.IsTrue(result.ConsistencyRatio > 0.10);
        Assert.IsFalse(result.IsConsistent);
    }

    [TestMethod]
    public void ReturnsDefensiveReadOnlyWeights()
    {
        var weights = new[] { 0.25, 0.75 };
        var result = new AhpResult(weights, 2, 0, 0, 0);
        weights[0] = 0.9;

        Assert.AreEqual(0.25, result.Weights[0], 0.000001);
        Assert.Throws<NotSupportedException>(
            () => ((IList<double>)result.Weights)[0] = 0.5);
    }

    [TestMethod]
    public void RejectsIncompleteMatrix()
    {
        var matrix = new AHPComparisonMatrix(3);
        matrix.SetComparison(0, 1, 3);

        Assert.Throws<InvalidOperationException>(
            () => new AhpCalculator().Calculate(matrix));
    }
}
