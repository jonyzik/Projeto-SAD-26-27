using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

public interface ISimulationService
{
    Task<CandidateParkCatalog> GetCandidateParksAsync(CancellationToken cancellationToken = default);
    WeightsPreview PreviewWeights(IReadOnlyDictionary<CriterionKind, int> importance, IReadOnlyCollection<CriterionKind>? mandatoryCriteria = null);
    OperationalCost EstimateCosts(decimal monthlyRentPerSquareMetre, CompanyNeeds needs);
    Task<SimulationOutcome> RunAsync(SimulationRequest request, CancellationToken cancellationToken = default);
    Task<Guid> SaveAsync(SimulationOutcome outcome, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
