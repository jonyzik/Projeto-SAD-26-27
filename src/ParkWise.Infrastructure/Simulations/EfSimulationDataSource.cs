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
                RequiredAreaSquareMetres = outcome.Needs.RequiredAreaSquareMetres,
                ContractDurationMonths = outcome.Needs.ContractDurationMonths
            }
        };

        foreach (var weight in outcome.Weights)
            simulation.CriterionWeights.Add(new SimulationCriterionWeight
            {
                Id = Guid.NewGuid(),
                EvaluationCriterionId = CriterionId(weight.Kind),
                Weight = weight.Weight,
                Importance = weight.Importance,
                IsMandatory = weight.IsMandatory
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
                EstimatedMonthlyCost = result.Costs?.Monthly,
                EstimatedContractCost = result.Costs?.ContractTotal,
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
            .Include(s => s.Results).ThenInclude(r => r.TechnologyPark)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (simulation is null) return null;

        var needs = new CompanyNeeds(
            simulation.Requirements.RequiredAreaSquareMetres, simulation.Requirements.ContractDurationMonths);

        // Simulações gravadas antes de existir a coluna Importance têm 0: a nota é reconstruída a partir do peso.
        var maxWeight = simulation.CriterionWeights.Select(w => w.Weight).DefaultIfEmpty(0m).Max();
        var weights = simulation.CriterionWeights
            .OrderBy(w => w.EvaluationCriterion.Kind)
            .Select(w => new CriterionWeight(
                w.EvaluationCriterion.Kind, w.EvaluationCriterion.Name,
                ImportanceScale.IsValid(w.Importance) ? w.Importance : ImportanceFromWeight(w.Weight, maxWeight),
                w.Weight, w.IsMandatory))
            .ToArray();
        var results = simulation.Results
            .OrderBy(r => r.IsEligible ? r.Ranking : int.MaxValue)
            .ThenBy(r => r.TechnologyPark.Name)
            .Select(r => new ParkSimulationResult(
                r.TechnologyParkId, r.TechnologyPark.Name, r.TechnologyPark.City,
                r.IsEligible ? r.Ranking : null, r.Score, r.IsEligible, Split(r.ExclusionReason),
                r.EstimatedMonthlyCost is decimal monthly
                    ? new OperationalCost(monthly, monthly * 12, r.EstimatedContractCost ?? monthly * needs.ContractDurationMonths)
                    : null,
                Contributions: [], Split(r.Strengths), Split(r.Weaknesses)))
            .ToArray();

        // Só a razão de consistência é persistida; λmax e IC são recalculados ao reexecutar a simulação.
        var consistency = new ConsistencyCheck(0m, 0m, simulation.ConsistencyRatio ?? 0m);

        return new SimulationOutcome(simulation.Id, simulation.Name, simulation.Description, simulation.CreatedAtUtc,
            needs, weights, consistency, results, IsDemonstration: false);
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

    private static int ImportanceFromWeight(decimal weight, decimal maxWeight) =>
        maxWeight <= 0
            ? ImportanceScale.Default
            : (int)Math.Clamp(Math.Round(weight / maxWeight * ImportanceScale.Maximum, MidpointRounding.AwayFromZero),
                ImportanceScale.Minimum, ImportanceScale.Maximum);

    private static string? Join(IReadOnlyList<string> values) =>
        values.Count == 0 ? null : string.Join(Separator, values);

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value) ? [] : value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
}
