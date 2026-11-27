using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Simulations;

namespace ParkWise.Tests;

[TestClass]
public sealed class OperationalCostCalculatorTests
{
    [TestMethod]
    public void MonthlyCostIsAreaTimesPricePerSquareMetre()
    {
        var result = OperationalCostCalculator.Calculate(12m, new CompanyNeeds(100m, 24));

        Assert.AreEqual(1200m, result.Monthly);
        Assert.AreEqual(14400m, result.Annual);
        Assert.AreEqual(28800m, result.ContractTotal);
    }

    [TestMethod]
    public void MonthlyCostIsRoundedToCents()
    {
        var result = OperationalCostCalculator.Calculate(9.999m, new CompanyNeeds(33.3m, 12));

        Assert.AreEqual(332.97m, result.Monthly);
        Assert.AreEqual(332.97m * 12, result.ContractTotal);
    }

    [TestMethod]
    public void RejectsNegativePrice()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            OperationalCostCalculator.Calculate(-1m, CompanyNeeds.Default));
    }
}
