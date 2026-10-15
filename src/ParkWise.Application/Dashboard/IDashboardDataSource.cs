namespace ParkWise.Application.Dashboard;

public interface IDashboardDataSource
{
    Task<DashboardData> LoadAsync(CancellationToken cancellationToken = default);
}
