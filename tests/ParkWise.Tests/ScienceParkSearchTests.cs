using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.ScienceParks;
using ParkWise.Core.Enums;

namespace ParkWise.Tests;

[TestClass]
public sealed class ScienceParkSearchTests
{
    private static readonly SciencePark[] Parks =
    [
        Park("Parque Atlântico", "Lisboa", 12m, CriterionKind.Wifi, CriterionKind.Parking),
        Park("Centro do Norte", "Porto", 18m, CriterionKind.Wifi),
        Park("Parque Interior", "Viseu", null, CriterionKind.Parking)
    ];

    [TestMethod]
    public void SearchesNameAndLocationIgnoringCaseAndAccents()
    {
        var result = ScienceParkSearch.Filter(Parks, "  atlantico ", "LISBOA", [], null, null);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Parque Atlântico", result[0].Name);
    }

    [TestMethod]
    public void CombinesRequiredServicesWithInclusivePriceRange()
    {
        var result = ScienceParkSearch.Filter(Parks, null, null,
            [CriterionKind.Wifi, CriterionKind.Parking], 12m, 12m);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Parque Atlântico", result[0].Name);
        Assert.AreEqual(0, ScienceParkSearch.Filter(Parks, null, null,
            [CriterionKind.Wifi, CriterionKind.Parking], 13m, 20m).Count);
    }

    [TestMethod]
    public void PriceFiltersExcludeUnknownPricesAndRejectReversedRange()
    {
        var priced = ScienceParkSearch.Filter(Parks, null, null, [], null, 20m);

        Assert.AreEqual(2, priced.Count);
        Assert.IsFalse(priced.Any(park => park.MonthlyRentPerSquareMetre is null));
        Assert.AreEqual(0, ScienceParkSearch.Filter(Parks, null, null, [], 20m, 10m).Count);
    }

    [TestMethod]
    public void SelectionKeepsUniqueParksAndOriginalOrderAcrossFilters()
    {
        var selection = new ScienceParkSelection();
        var changes = 0;
        selection.Changed += (_, _) => changes++;
        selection.SetSelected(Parks[1].Id, true);
        selection.SetSelected(Parks[0].Id, true);
        selection.SetSelected(Parks[1].Id, true);

        Assert.AreEqual(2, selection.Count);
        CollectionAssert.AreEqual(new[] { Parks[1].Id, Parks[0].Id }, selection.SelectedIds.ToArray());
        Assert.AreEqual(2, changes);

        _ = ScienceParkSearch.Filter(Parks, "Interior", null, [], null, null);
        Assert.IsTrue(selection.Contains(Parks[0].Id));

        selection.SetSelected(Parks[1].Id, false);
        Assert.AreEqual(1, selection.Count);
        Assert.AreEqual(3, changes);
    }

    private static SciencePark Park(string name, string city, decimal? price, params CriterionKind[] services) =>
        new(Guid.NewGuid(), name, city, "Descrição de teste", price,
            services.ToDictionary(service => service, _ => true));
}
