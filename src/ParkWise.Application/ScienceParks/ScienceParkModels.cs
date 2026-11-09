using ParkWise.Core.Enums;

namespace ParkWise.Application.ScienceParks;

public sealed record SciencePark(
    Guid Id,
    string Name,
    string City,
    string Description,
    decimal? MonthlyRentPerSquareMetre,
    IReadOnlyDictionary<CriterionKind, bool> AvailableServices);

public sealed record ScienceParkCatalogData(
    IReadOnlyList<SciencePark> Parks,
    bool IsDemonstration);

public interface IScienceParkCatalog
{
    Task<ScienceParkCatalogData> LoadAsync(CancellationToken cancellationToken = default);
}
