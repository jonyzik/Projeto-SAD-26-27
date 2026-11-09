using ParkWise.Application.Ahp;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

/// <summary>
/// Executa uma simulação: valida o pedido, calcula os pesos dos critérios com o AHP, calcula os custos
/// operacionais, exclui os parques que não satisfazem as necessidades da empresa e ordena os restantes.
/// O critério «Preço competitivo da área alugada» é avaliado com o custo operacional mensal calculado
/// (menor custo → maior prioridade), integrando os custos no modelo AHP.
/// </summary>
public sealed class SimulationEngine(IAhpEngine ahpEngine)
{
    private const int MaxHighlights = 3;
    private const double HighlightThreshold = 0.005;

    private static readonly IReadOnlyList<CriterionKind> Criteria =
        EvaluationCriteria.Required.Select(criterion => criterion.Kind).ToArray();

    private static readonly IReadOnlyDictionary<CriterionKind, string> CriterionNames =
        EvaluationCriteria.Required.ToDictionary(criterion => criterion.Kind, criterion => criterion.Name);

    public static string NameOf(CriterionKind kind) => CriterionNames[kind];

    public WeightsPreview CalculateWeights(
        IEnumerable<CriterionComparison> comparisons, IReadOnlyCollection<CriterionKind>? mandatoryCriteria = null)
    {
        var matrix = PairwiseComparisonMatrix.Build(Criteria, comparisons);
        var priorities = ahpEngine.CalculatePriorities(matrix);
        if (priorities.Weights.Count != Criteria.Count)
            throw new InvalidOperationException("O motor AHP devolveu um número de pesos inesperado.");

        var mandatory = mandatoryCriteria ?? [];
        var weights = Criteria
            .Select((kind, i) => new CriterionWeight(
                kind, CriterionNames[kind], Round((decimal)priorities.Weights[i], 6), mandatory.Contains(kind)))
            .ToArray();

        return new WeightsPreview(weights, new ConsistencyCheck(
            Round((decimal)priorities.LambdaMax, 4),
            Round((decimal)priorities.ConsistencyIndex, 4),
            Round((decimal)priorities.ConsistencyRatio, 4)));
    }

    public SimulationOutcome Run(SimulationRequest request, IReadOnlyList<CandidatePark> availableParks,
        DateTime executedAtUtc, bool isDemonstration = false)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(availableParks);

        var errors = SimulationRequestValidator.Validate(request).ToList();
        var parksById = availableParks.ToDictionary(park => park.Id);
        var selectedIds = request.CandidateParkIds.Distinct().ToArray();
        if (selectedIds.Any(id => !parksById.ContainsKey(id)))
            errors.Add("Um ou mais parques selecionados já não existem no catálogo.");
        if (errors.Count > 0) throw new SimulationValidationException(errors);

        var preview = CalculateWeights(request.Comparisons, request.MandatoryCriteria);
        if (!preview.Consistency.IsAcceptable)
            throw new SimulationValidationException(
            [
                $"As comparações entre critérios são inconsistentes (RC = {preview.Consistency.ConsistencyRatio:0.000}; " +
                $"o máximo aceitável é {ConsistencyCheck.AcceptableRatio:0.00}). Reveja as comparações antes de executar."
            ]);

        var needs = request.Parameters.Needs;
        var evaluated = selectedIds
            .Select(id => parksById[id])
            .Select(park => Evaluate(park, request, needs))
            .ToArray();

        var eligible = evaluated.Where(item => item.Reasons.Count == 0).ToArray();
        var globalPriorities = new double[eligible.Length];
        var localPriorities = new double[eligible.Length, Criteria.Count];

        if (eligible.Length > 0)
        {
            for (var c = 0; c < Criteria.Count; c++)
            {
                var kind = Criteria[c];
                var raw = eligible.Select(item => kind == CriterionKind.CompetitiveRentPerSquareMetre
                    ? 1.0 / Math.Max((double)item.Costs!.MonthlyTotal, 0.01)
                    : Math.Max(0, (double)item.Park.Scores[kind])).ToArray();
                var sum = raw.Sum();
                for (var a = 0; a < eligible.Length; a++)
                    localPriorities[a, c] = sum > 0 ? raw[a] / sum : 1.0 / eligible.Length;
            }

            var weights = preview.Weights.Select(weight => (double)weight.Weight).ToArray();
            var synthesized = ahpEngine.Synthesize(weights, localPriorities);
            for (var a = 0; a < eligible.Length; a++) globalPriorities[a] = synthesized[a];
        }

        var ranking = eligible
            .Select((item, index) => (item, index))
            .OrderByDescending(x => globalPriorities[x.index])
            .ThenBy(x => x.item.Costs!.MonthlyTotal)
            .ThenBy(x => x.item.Park.Name, StringComparer.CurrentCulture)
            .Select((x, position) => (x.item.Park.Id, Rank: position + 1))
            .ToDictionary(x => x.Id, x => x.Rank);

        var results = new List<ParkSimulationResult>();
        for (var a = 0; a < eligible.Length; a++)
        {
            var item = eligible[a];
            var contributions = Criteria.Select((kind, c) => new CriterionContribution(
                kind, CriterionNames[kind],
                Round((decimal)localPriorities[a, c], 6),
                Round((decimal)(localPriorities[a, c] * (double)preview.Weights[c].Weight), 6))).ToArray();
            var (strengths, weaknesses) = Highlights(a, localPriorities, preview.Weights, eligible.Length);

            results.Add(new ParkSimulationResult(
                item.Park.Id, item.Park.Name, item.Park.City, ranking[item.Park.Id],
                Round((decimal)globalPriorities[a], 6), true, [],
                item.Costs!.MonthlyTotal, item.Costs.SetupCost, item.Costs.ContractTotal, item.Costs,
                contributions, strengths, weaknesses));
        }

        results.AddRange(evaluated.Where(item => item.Reasons.Count > 0).Select(item => new ParkSimulationResult(
            item.Park.Id, item.Park.Name, item.Park.City, null, 0m, false, item.Reasons,
            item.Costs?.MonthlyTotal, item.Costs?.SetupCost, item.Costs?.ContractTotal, item.Costs,
            [], [], [])));

        var ordered = results
            .OrderBy(result => result.Ranking ?? int.MaxValue)
            .ThenBy(result => result.ParkName, StringComparer.CurrentCulture)
            .ToArray();

        return new SimulationOutcome(
            Id: null,
            Name: request.Name.Trim(),
            Description: string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ExecutedAtUtc: executedAtUtc,
            Parameters: request.Parameters,
            Weights: preview.Weights,
            Consistency: preview.Consistency,
            Comparisons: request.Comparisons.ToArray(),
            Results: ordered,
            IsDemonstration: isDemonstration);
    }

    private static EvaluatedPark Evaluate(CandidatePark park, SimulationRequest request, CompanyNeeds needs)
    {
        var reasons = new List<string>();
        OperationalCostBreakdown? costs = null;

        if (park.MonthlyRentPerSquareMetre is not > 0)
            reasons.Add("Sem preço por m² válido.");
        else
            costs = OperationalCostCalculator.Calculate(park.MonthlyRentPerSquareMetre.Value, needs, request.Parameters.Costs);

        var missing = Criteria
            .Where(kind => kind != CriterionKind.CompetitiveRentPerSquareMetre && !park.Scores.ContainsKey(kind))
            .ToArray();
        if (missing.Length > 0)
            reasons.Add($"Avaliação incompleta: falta {string.Join(", ", missing.Select(kind => CriterionNames[kind]))}.");

        foreach (var kind in request.MandatoryCriteria.Where(kind => kind != CriterionKind.CompetitiveRentPerSquareMetre))
            if (park.Scores.TryGetValue(kind, out var score) && score <= 0)
                reasons.Add($"Não cumpre o critério obrigatório «{CriterionNames[kind]}».");

        if (costs is not null && needs.MaxMonthlyBudget is decimal budget && costs.MonthlyTotal > budget)
            reasons.Add($"Custo mensal ({costs.MonthlyTotal:N2} €) acima do orçamento ({budget:N2} €).");

        if (!string.IsNullOrWhiteSpace(needs.PreferredCity) &&
            !string.Equals(park.City.Trim(), needs.PreferredCity.Trim(), StringComparison.CurrentCultureIgnoreCase))
            reasons.Add($"Localização diferente da pretendida ({needs.PreferredCity.Trim()}).");

        return new EvaluatedPark(park, costs, reasons);
    }

    /// <summary>
    /// Pontos fortes e fracos: critérios em que a prioridade local do parque mais se afasta da média
    /// das alternativas elegíveis (1/n), ponderados pelo peso do critério.
    /// </summary>
    private static (IReadOnlyList<string> Strengths, IReadOnlyList<string> Weaknesses) Highlights(
        int alternative, double[,] local, IReadOnlyList<CriterionWeight> weights, int alternatives)
    {
        if (alternatives < 2) return ([], []);
        var average = 1.0 / alternatives;
        var deltas = weights
            .Select((weight, c) => (weight.Name, Delta: (double)weight.Weight * (local[alternative, c] - average) / average))
            .ToArray();

        var strengths = deltas.Where(x => x.Delta > HighlightThreshold)
            .OrderByDescending(x => x.Delta).Take(MaxHighlights).Select(x => x.Name).ToArray();
        var weaknesses = deltas.Where(x => x.Delta < -HighlightThreshold)
            .OrderBy(x => x.Delta).Take(MaxHighlights).Select(x => x.Name).ToArray();
        return (strengths, weaknesses);
    }

    private static decimal Round(decimal value, int decimals) =>
        Math.Round(value, decimals, MidpointRounding.AwayFromZero);

    private sealed record EvaluatedPark(CandidatePark Park, OperationalCostBreakdown? Costs, List<string> Reasons);
}
