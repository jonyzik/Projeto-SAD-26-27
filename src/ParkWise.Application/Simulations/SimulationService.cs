using ParkWise.Application.Abstractions;
using ParkWise.Application.Ahp;
using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

public sealed class SimulationService(
    ISimulationDataSource dataSource,
    ISimulationParametersStore parametersStore,
    IAhpEngine ahpEngine,
    ICurrentUserAccessor currentUser,
    TimeProvider? timeProvider = null) : ISimulationService
{
    private readonly SimulationEngine _engine = new(ahpEngine);
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public Task<CandidateParkCatalog> GetCandidateParksAsync(CancellationToken cancellationToken = default) =>
        dataSource.LoadCandidateParksAsync(cancellationToken);

    public WeightsPreview PreviewWeights(IEnumerable<CriterionComparison> comparisons,
        IReadOnlyCollection<CriterionKind>? mandatoryCriteria = null) =>
        _engine.CalculateWeights(comparisons, mandatoryCriteria);

    public OperationalCostBreakdown EstimateCosts(decimal monthlyRentPerSquareMetre, SimulationParameters parameters) =>
        OperationalCostCalculator.Calculate(monthlyRentPerSquareMetre, parameters.Needs, parameters.Costs);

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

    public Task<SimulationParameters> GetDefaultParametersAsync(CancellationToken cancellationToken = default) =>
        parametersStore.LoadAsync(cancellationToken);

    public Task SaveDefaultParametersAsync(SimulationParameters parameters, CancellationToken cancellationToken = default)
    {
        var errors = SimulationRequestValidator.ValidateParameters(parameters);
        if (errors.Count > 0) throw new SimulationValidationException(errors);
        return parametersStore.SaveAsync(parameters, cancellationToken);
    }

    public async Task<SimulationParameters> ResetDefaultParametersAsync(CancellationToken cancellationToken = default)
    {
        await parametersStore.ResetAsync(cancellationToken);
        return await parametersStore.LoadAsync(cancellationToken);
    }
}
