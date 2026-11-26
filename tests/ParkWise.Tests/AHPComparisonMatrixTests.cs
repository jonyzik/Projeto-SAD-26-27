using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Core.AHP;

namespace ParkWise.Tests;

[TestClass]
public sealed class AHPComparisonMatrixTests
{
    [TestMethod]
    public void StoresTheReciprocalForEachComparison()
    {
        var matrix = new AHPComparisonMatrix(3);

        matrix.SetComparison(0, 1, 4);

        Assert.AreEqual(4, matrix[0, 1], 0.000001);
        Assert.AreEqual(0.25, matrix[1, 0], 0.000001);
        Assert.IsFalse(matrix.IsComplete);
    }

    [TestMethod]
    public void RejectsInvalidIndicesAndValues()
    {
        var matrix = new AHPComparisonMatrix(2);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => matrix.SetComparison(-1, 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => matrix.SetComparison(0, 2, 2));
        Assert.Throws<ArgumentException>(
            () => matrix.SetComparison(0, 0, 1));
        Assert.Throws<ArgumentException>(
            () => matrix.SetComparison(0, 1, double.NaN));
        Assert.Throws<ArgumentException>(
            () => matrix.SetComparison(0, 1, 1.5));
    }

    [TestMethod]
    public void RejectsUnsupportedMatrixSizes()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AHPComparisonMatrix(0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AHPComparisonMatrix(16));
    }
}
