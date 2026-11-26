using ParkWise.Application.Abstractions;
using ParkWise.Application.Ahp;
using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

public sealed class SimulationService(
    ISimulationDataSource dataSource,
    IAhpEngine ahpEngine,
    ICurrentUserAccessor currentUser,
    TimeProvider? timeProvider = null) : ISimulationService
{
    private readonly SimulationEngine _engine = new(ahpEngine);
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public Task<CandidateParkCatalog> GetCandidateParksAsync(CancellationToken cancellationToken = default) =>
        dataSource.LoadCandidateParksAsync(cancellationToken);

    public WeightsPreview PreviewWeights(IReadOnlyDictionary<CriterionKind, int> importance,
        IReadOnlyCollection<CriterionKind>? mandatoryCriteria = null) =>
        _engine.CalculateWeights(importance, mandatoryCriteria);

    public OperationalCost EstimateCosts(decimal monthlyRentPerSquareMetre, CompanyNeeds needs) =>
        OperationalCostCalculator.Calculate(monthlyRentPerSquareMetre, needs);

    public async Task<SimulationOutcome> RunAsync(SimulationRequest request, CancellationToken cancellationToken = default)
    {
        var catalog = await dataSource.LoadCandidateParksAsync(cancellationToken);
        return _engine.Run(request, catalog.Parks, _time.GetUtcNow().UtcDateTime, catalog.IsDemonstration);
    }

    public Task<Guid> SaveAsync(SimulationOutcome outcome, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        if (outcome.Id is not null)
            throw new InvalidOperationException("Esta simulação já foi guardada.");
        return dataSource.SaveAsync(outcome, currentUser.CurrentUserId, cancellationToken);
    }

    public Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default) =>
        dataSource.ListAsync(cancellationToken);

    public Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dataSource.GetAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        dataSource.DeleteAsync(id, cancellationToken);
}
