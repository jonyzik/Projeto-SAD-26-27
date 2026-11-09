namespace ParkWise.Application.Simulations;

/// <summary>Acesso aos parques candidatos e às simulações guardadas (base de dados ou demonstração).</summary>
public interface ISimulationDataSource
{
    Task<CandidateParkCatalog> LoadCandidateParksAsync(CancellationToken cancellationToken = default);
    Task<Guid> SaveAsync(SimulationOutcome outcome, Guid? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
