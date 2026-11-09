using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

public interface ISimulationService
{
    Task<CandidateParkCatalog> GetCandidateParksAsync(CancellationToken cancellationToken = default);
    WeightsPreview PreviewWeights(IEnumerable<CriterionComparison> comparisons, IReadOnlyCollection<CriterionKind>? mandatoryCriteria = null);
    OperationalCostBreakdown EstimateCosts(decimal monthlyRentPerSquareMetre, SimulationParameters parameters);
    Task<SimulationOutcome> RunAsync(SimulationRequest request, CancellationToken cancellationToken = default);
    Task<Guid> SaveAsync(SimulationOutcome outcome, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SimulationParameters> GetDefaultParametersAsync(CancellationToken cancellationToken = default);
    Task SaveDefaultParametersAsync(SimulationParameters parameters, CancellationToken cancellationToken = default);
    Task<SimulationParameters> ResetDefaultParametersAsync(CancellationToken cancellationToken = default);
}
