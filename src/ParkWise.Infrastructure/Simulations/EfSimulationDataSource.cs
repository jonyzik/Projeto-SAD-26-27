using Microsoft.EntityFrameworkCore;
using ParkWise.Application.Simulations;
using ParkWise.Core.Entities;
using ParkWise.Core.Enums;
using ParkWise.Core.Simulations;
using ParkWise.Infrastructure.Persistence;

namespace ParkWise.Infrastructure.Simulations;

public sealed class EfSimulationDataSource(IDbContextFactory<ParkWiseDbContext> contextFactory) : ISimulationDataSource
{
    private const char Separator = '\n';

    public async Task<CandidateParkCatalog> LoadCandidateParksAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var parks = await context.TechnologyParks.AsNoTracking()
            .OrderBy(park => park.Name)
            .Select(park => new
            {
                park.Id,
                park.Name,
                park.City,
                park.MonthlyRentPerSquareMetre,
                Scores = park.Evaluations
                    .Select(evaluation => new { evaluation.EvaluationCriterion.Kind, evaluation.Score })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new CandidateParkCatalog(
            parks.Select(park => new CandidatePark(
                park.Id, park.Name, park.City, park.MonthlyRentPerSquareMetre,
                park.Scores.GroupBy(score => score.Kind).ToDictionary(group => group.Key, group => group.First().Score)))
            .ToArray(),
            IsDemonstration: false);
    }

    public async Task<Guid> SaveAsync(SimulationOutcome outcome, Guid? userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var criterionIds = await context.EvaluationCriteria.AsNoTracking()
            .ToDictionaryAsync(criterion => criterion.Kind, criterion => criterion.Id, cancellationToken);

        int CriterionId(CriterionKind kind) => criterionIds.TryGetValue(kind, out var id)
            ? id
            : throw new InvalidOperationException("Os critérios de avaliação não existem na base de dados. Aplique as migrações.");

        var needs = outcome.Parameters.Needs;
        var costs = outcome.Parameters.Costs;
        var simulation = new Simulation
        {
            Id = Guid.NewGuid(),
            Name = outcome.Name,
            Description = outcome.Description,
            UserId = userId,
            CreatedAtUtc = outcome.ExecutedAtUtc,
            ConsistencyRatio = outcome.Consistency.ConsistencyRatio,
            Requirements = new CompanyRequirements
            {
                RequiredAreaSquareMetres = needs.RequiredAreaSquareMetres,
                EmployeeCount = needs.EmployeeCount,
                ContractDurationMonths = needs.ContractDurationMonths,
                MaxMonthlyBudget = needs.MaxMonthlyBudget,
                PreferredCity = needs.PreferredCity
            },
            CostSettings = new OperationalCostSettings
            {
                ServiceChargePerSquareMetre = costs.ServiceChargePerSquareMetre,
                UtilitiesPerSquareMetre = costs.UtilitiesPerSquareMetre,
                ServicesPerEmployee = costs.ServicesPerEmployee,
                ParkingSpacesRatio = costs.ParkingSpacesRatio,
                ParkingCostPerSpace = costs.ParkingCostPerSpace,
                SetupCostPerSquareMetre = costs.SetupCostPerSquareMetre,
                ContingencyPercentage = costs.ContingencyPercentage
            }
        };

        foreach (var weight in outcome.Weights)
            simulation.CriterionWeights.Add(new SimulationCriterionWeight
            {
                Id = Guid.NewGuid(),
                EvaluationCriterionId = CriterionId(weight.Kind),
                Weight = weight.Weight,
                IsMandatory = weight.IsMandatory
            });

        foreach (var comparison in outcome.Comparisons)
            simulation.AhpComparisons.Add(new AhpComparison
            {
                Id = Guid.NewGuid(),
                LeftCriterionId = CriterionId(comparison.Left),
                RightCriterionId = CriterionId(comparison.Right),
                PreferenceValue = Math.Round(comparison.Value, 8)
            });

        foreach (var result in outcome.Results)
            simulation.Results.Add(new SimulationResult
            {
                Id = Guid.NewGuid(),
                TechnologyParkId = result.ParkId,
                Ranking = result.Ranking ?? 0,
                Score = result.Score,
                IsEligible = result.IsEligible,
                ExclusionReason = Join(result.ExclusionReasons),
                EstimatedMonthlyCost = result.MonthlyCost,
                EstimatedSetupCost = result.SetupCost,
                EstimatedContractCost = result.ContractCost,
                Strengths = Join(result.Strengths),
                Weaknesses = Join(result.Weaknesses)
            });

        context.Simulations.Add(simulation);
        await context.SaveChangesAsync(cancellationToken);
        return simulation.Id;
    }

    public async Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var rows = await context.Simulations.AsNoTracking()
            .OrderByDescending(simulation => simulation.CreatedAtUtc)
            .Select(simulation => new
            {
                simulation.Id,
                simulation.Name,
                simulation.CreatedAtUtc,
                simulation.ConsistencyRatio,
                CandidateCount = simulation.Results.Count(),
                EligibleCount = simulation.Results.Count(result => result.IsEligible),
                Best = simulation.Results
                    .Where(result => result.Ranking == 1)
                    .Select(result => new { result.TechnologyPark.Name, result.EstimatedMonthlyCost })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new SimulationListItem(
            row.Id, row.Name, row.CreatedAtUtc, row.CandidateCount, row.EligibleCount,
            row.Best?.Name, row.Best?.EstimatedMonthlyCost, row.ConsistencyRatio)).ToArray();
    }

    public async Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var simulation = await context.Simulations.AsNoTracking()
            .Include(s => s.CriterionWeights).ThenInclude(w => w.EvaluationCriterion)
            .Include(s => s.AhpComparisons).ThenInclude(c => c.LeftCriterion)
            .Include(s => s.AhpComparisons).ThenInclude(c => c.RightCriterion)
            .Include(s => s.Results).ThenInclude(r => r.TechnologyPark)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (simulation is null) return null;

        var requirements = simulation.Requirements;
        var settings = simulation.CostSettings;
        var parameters = new SimulationParameters(
            new CompanyNeeds(requirements.RequiredAreaSquareMetres, requirements.EmployeeCount,
                requirements.ContractDurationMonths, requirements.MaxMonthlyBudget, requirements.PreferredCity),
            new CostParameters(settings.ServiceChargePerSquareMetre, settings.UtilitiesPerSquareMetre,
                settings.ServicesPerEmployee, settings.ParkingSpacesRatio, settings.ParkingCostPerSpace,
                settings.SetupCostPerSquareMetre, settings.ContingencyPercentage));

        var weights = simulation.CriterionWeights
            .OrderBy(w => w.EvaluationCriterion.Kind)
            .Select(w => new CriterionWeight(w.EvaluationCriterion.Kind, w.EvaluationCriterion.Name, w.Weight, w.IsMandatory))
            .ToArray();
        var comparisons = simulation.AhpComparisons
            .Select(c => new CriterionComparison(c.LeftCriterion.Kind, c.RightCriterion.Kind, c.PreferenceValue))
            .ToArray();
        var results = simulation.Results
            .OrderBy(r => r.IsEligible ? r.Ranking : int.MaxValue)
            .ThenBy(r => r.TechnologyPark.Name)
            .Select(r => new ParkSimulationResult(
                r.TechnologyParkId, r.TechnologyPark.Name, r.TechnologyPark.City,
                r.IsEligible ? r.Ranking : null, r.Score, r.IsEligible, Split(r.ExclusionReason),
                r.EstimatedMonthlyCost, r.EstimatedSetupCost, r.EstimatedContractCost,
                CostBreakdown: null, Contributions: [], Split(r.Strengths), Split(r.Weaknesses)))
            .ToArray();

        // Só a razão de consistência é persistida; λmax e IC são recalculados ao reexecutar a simulação.
        var consistency = new ConsistencyCheck(0m, 0m, simulation.ConsistencyRatio ?? 0m);

        return new SimulationOutcome(simulation.Id, simulation.Name, simulation.Description, simulation.CreatedAtUtc,
            parameters, weights, consistency, comparisons, results, IsDemonstration: false);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var simulation = await context.Simulations.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (simulation is null) return false;
        context.Simulations.Remove(simulation);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Join(IReadOnlyList<string> values) =>
        values.Count == 0 ? null : string.Join(Separator, values);

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value) ? [] : value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
}
