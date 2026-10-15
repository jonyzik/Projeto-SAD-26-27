using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Core;
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
            Enum.GetValues<CriterionKind>().ToList(),
            EvaluationCriteria.Required.Select(x => x.Kind).ToList());
    }
}
