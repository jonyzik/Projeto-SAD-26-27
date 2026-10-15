using System.Globalization;
using ParkWise.Core.Enums;

namespace ParkWise.Application.ScienceParks;

public static class ScienceParkSearch
{
    private static readonly CompareInfo PortugueseComparison = CultureInfo.GetCultureInfo("pt-PT").CompareInfo;

    public static IReadOnlyList<SciencePark> Filter(
        IEnumerable<SciencePark> parks,
        string? name,
        string? location,
        IReadOnlyCollection<CriterionKind> requiredServices,
        decimal? minimumPrice,
        decimal? maximumPrice)
    {
        if (minimumPrice > maximumPrice) return [];

        return parks.Where(park =>
                Contains(park.Name, name) &&
                Contains(park.City, location) &&
                (!minimumPrice.HasValue || park.MonthlyRentPerSquareMetre >= minimumPrice) &&
                (!maximumPrice.HasValue || park.MonthlyRentPerSquareMetre <= maximumPrice) &&
                requiredServices.All(service =>
                    park.AvailableServices.TryGetValue(service, out var available) && available))
            .OrderBy(park => park.Name, StringComparer.Create(CultureInfo.GetCultureInfo("pt-PT"), true))
            .ToArray();
    }

    private static bool Contains(string value, string? search) =>
        string.IsNullOrWhiteSpace(search) ||
        PortugueseComparison.IndexOf(value, search.Trim(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
}
