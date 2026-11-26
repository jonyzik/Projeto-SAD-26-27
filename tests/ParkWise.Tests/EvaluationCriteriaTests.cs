using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Core;
using ParkWise.Core.AHP;
using ParkWise.Core.Enums;

namespace ParkWise.Tests;

[TestClass]
public sealed class EvaluationCriteriaTests
{
    [TestMethod]
    public void RequiredCriteriaContainNineUniqueKinds()
    {
        Assert.AreEqual(9, EvaluationCriteria.Required.Count);
        Assert.AreEqual(9, EvaluationCriteria.Required.Select(x => x.Kind).Distinct().Count());
        CollectionAssert.AreEquivalent(
            Enum.GetValues<CriterionKind>()
                .Where(kind => kind != CriterionKind.GeographicLocation)
                .ToList(),
            EvaluationCriteria.Required.Select(x => x.Kind).ToList());
    }

    [TestMethod]
    public void AhpCriteriaContainTenUniqueKindsIncludingGeographicLocation()
    {
        Assert.AreEqual(10, AHPCriteria.Required.Count);
        Assert.AreEqual(10, AHPCriteria.Required.Select(x => x.Kind).Distinct().Count());
        CollectionAssert.AreEquivalent(
            Enum.GetValues<CriterionKind>().ToList(),
            AHPCriteria.Required.Select(x => x.Kind).ToList());
    }
}
