using ParkWise.Core.Enums;

namespace ParkWise.Application.ScienceParks;

public sealed record SciencePark(
    Guid Id,
    string Name,
    string City,
    string Description,
    decimal? MonthlyRentPerSquareMetre,
    IReadOnlyDictionary<CriterionKind, bool> AvailableServices,
    ScienceParkCoordinates? Coordinates = null);

public sealed record ScienceParkCoordinates(double Latitude, double Longitude)
{
    public bool IsValid =>
        double.IsFinite(Latitude) &&
        double.IsFinite(Longitude) &&
        Latitude is >= -90 and <= 90 &&
        Longitude is >= -180 and <= 180;
}

public sealed record ScienceParkCatalogData(
    IReadOnlyList<SciencePark> Parks,
    bool IsDemonstration);

public interface IScienceParkCatalog
{
    Task<ScienceParkCatalogData> LoadAsync(CancellationToken cancellationToken = default);
}
