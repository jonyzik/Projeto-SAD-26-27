using ParkWise.Application.Dashboard;
using ParkWise.Application.ScienceParks;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.App.Demo;

/// <summary>Adapta o catálogo fictício da interface aos indicadores de demonstração.</summary>
public sealed class DemoCatalogDashboardDataSource(IScienceParkCatalog catalog) : IDashboardDataSource
{
    public async Task<DashboardData> LoadAsync(CancellationToken cancellationToken = default)
    {
        var data = await catalog.LoadAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var parks = data.Parks.Select((park, index) => new DashboardPark(
            park.Id,
            park.Name,
            park.City,
            park.MonthlyRentPerSquareMetre,
            now.AddDays(-index),
            EvaluationCriteria.Required.Count(criterion =>
                criterion.Kind == CriterionKind.CompetitiveRentPerSquareMetre
                    ? park.MonthlyRentPerSquareMetre is > 0
                    : park.AvailableServices.ContainsKey(criterion.Kind)))).ToArray();

        return new DashboardData(parks, [], data.IsDemonstration);
    }
}
