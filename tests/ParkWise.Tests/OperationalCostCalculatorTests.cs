using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Simulations;

namespace ParkWise.Tests;

[TestClass]
public sealed class OperationalCostCalculatorTests
{
    [TestMethod]
    public void CalculatesEveryComponentOfTheMonthlyAndContractCost()
    {
        var needs = new CompanyNeeds(100m, 7, 24, null, null);
        var costs = new CostParameters(
            ServiceChargePerSquareMetre: 2m,
            UtilitiesPerSquareMetre: 1.5m,
            ServicesPerEmployee: 30m,
            ParkingSpacesRatio: 0.5m,
            ParkingCostPerSpace: 40m,
            SetupCostPerSquareMetre: 20m,
            ContingencyPercentage: 10m);

        var result = OperationalCostCalculator.Calculate(12m, needs, costs);

        Assert.AreEqual(1200m, result.Rent);
        Assert.AreEqual(200m, result.ServiceCharges);
        Assert.AreEqual(150m, result.Utilities);
        Assert.AreEqual(210m, result.EmployeeServices);
        Assert.AreEqual(4, result.ParkingSpaces);          // ⌈7 × 0,5⌉
        Assert.AreEqual(160m, result.Parking);
        Assert.AreEqual(192m, result.Contingency);         // 10 % de 1920
        Assert.AreEqual(2112m, result.MonthlyTotal);
        Assert.AreEqual(25344m, result.AnnualTotal);
        Assert.AreEqual(2000m, result.SetupCost);
        Assert.AreEqual(2112m * 24 + 2000m, result.ContractTotal);
    }

    [TestMethod]
    public void ZeroParametersLeaveOnlyTheRent()
    {
        var result = OperationalCostCalculator.Calculate(
            9.99m, new CompanyNeeds(50m, 3, 12, null, null), new CostParameters(0, 0, 0, 0, 0, 0, 0));

        Assert.AreEqual(499.50m, result.Rent);
        Assert.AreEqual(499.50m, result.MonthlyTotal);
        Assert.AreEqual(0, result.ParkingSpaces);
        Assert.AreEqual(499.50m * 12, result.ContractTotal);
    }

    [TestMethod]
    public void RejectsNegativeRent()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            OperationalCostCalculator.Calculate(-1m, CompanyNeeds.Default, CostParameters.Default));
    }
}
