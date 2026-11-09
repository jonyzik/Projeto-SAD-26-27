using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Simulations;

namespace ParkWise.Tests;

[TestClass]
public sealed class SaatyScaleTests
{
    [TestMethod]
    public void SliderPositionsMapToSaatyValuesAndBack()
    {
        Assert.AreEqual(1m, SaatyScale.FromSliderPosition(0));
        Assert.AreEqual(9m, SaatyScale.FromSliderPosition(-8));
        Assert.AreEqual(3m, SaatyScale.FromSliderPosition(-2));
        Assert.AreEqual(1m / 5m, SaatyScale.FromSliderPosition(4));
        Assert.AreEqual(1m / 9m, SaatyScale.FromSliderPosition(8));

        for (var position = -8; position <= 8; position++)
            Assert.AreEqual(position, SaatyScale.ToSliderPosition(SaatyScale.FromSliderPosition(position)));
    }

    [TestMethod]
    public void ValidatesTheScaleLimits()
    {
        Assert.IsTrue(SaatyScale.IsValid(9m));
        Assert.IsTrue(SaatyScale.IsValid(1m / 9m));
        Assert.IsFalse(SaatyScale.IsValid(10m));
        Assert.IsFalse(SaatyScale.IsValid(0.1m));
        Assert.AreEqual("Fortemente mais importante", SaatyScale.Describe(5));
    }
}
