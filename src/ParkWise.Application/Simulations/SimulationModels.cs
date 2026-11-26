using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

/// <summary>
/// Dados da empresa necessários para calcular os custos operacionais: a área a arrendar
/// e a duração do contrato.
/// </summary>
public sealed record CompanyNeeds(decimal RequiredAreaSquareMetres, int ContractDurationMonths)
{
    public static CompanyNeeds Default { get; } = new(120m, 36);
}

public sealed record SimulationRequest(
    string Name,
    string? Description,
    CompanyNeeds Needs,
    IReadOnlyList<Guid> CandidateParkIds,
    IReadOnlyDictionary<CriterionKind, int> CriterionImportance,
    IReadOnlyCollection<CriterionKind> MandatoryCriteria);

/// <summary>
/// Parque disponível para simulação. As pontuações por critério vêm do módulo de parques;
/// a escala é livre (por exemplo 0–10), porque o AHP normaliza os valores entre alternativas.
/// </summary>
public sealed record CandidatePark(
    Guid Id,
    string Name,
    string City,
    decimal? MonthlyRentPerSquareMetre,
    IReadOnlyDictionary<CriterionKind, decimal> Scores);

public sealed record CandidateParkCatalog(IReadOnlyList<CandidatePark> Parks, bool IsDemonstration);

/// <summary>Custos operacionais de arrendar a área pretendida num parque, em euros.</summary>
public sealed record OperationalCost(decimal Monthly, decimal Annual, decimal ContractTotal);

/// <summary>Peso calculado pelo AHP para um critério, a partir da importância (1 a 9) indicada pelo utilizador.</summary>
public sealed record CriterionWeight(CriterionKind Kind, string Name, int Importance, decimal Weight, bool IsMandatory);

public sealed record ConsistencyCheck(decimal LambdaMax, decimal ConsistencyIndex, decimal ConsistencyRatio)
{
    public const decimal AcceptableRatio = 0.10m;
    public bool IsAcceptable => ConsistencyRatio <= AcceptableRatio;
}

public sealed record WeightsPreview(IReadOnlyList<CriterionWeight> Weights, ConsistencyCheck Consistency);

public sealed record CriterionContribution(
    CriterionKind Kind,
    string Name,
    decimal LocalPriority,
    decimal WeightedContribution);

public sealed record ParkSimulationResult(
    Guid ParkId,
    string ParkName,
    string City,
    int? Ranking,
    decimal Score,
    bool IsEligible,
    IReadOnlyList<string> ExclusionReasons,
    OperationalCost? Costs,
    IReadOnlyList<CriterionContribution> Contributions,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses);

public sealed record SimulationOutcome(
    Guid? Id,
    string Name,
    string? Description,
    DateTime ExecutedAtUtc,
    CompanyNeeds Needs,
    IReadOnlyList<CriterionWeight> Weights,
    ConsistencyCheck Consistency,
    IReadOnlyList<ParkSimulationResult> Results,
    bool IsDemonstration)
{
    public IReadOnlyList<ParkSimulationResult> RankedResults =>
        Results.Where(result => result.Ranking is not null).OrderBy(result => result.Ranking).ToArray();

    public ParkSimulationResult? BestResult => RankedResults.FirstOrDefault();

    public IReadOnlyList<Guid> CandidateParkIds => Results.Select(result => result.ParkId).ToArray();

    public IReadOnlyCollection<CriterionKind> MandatoryCriteria =>
        Weights.Where(weight => weight.IsMandatory).Select(weight => weight.Kind).ToArray();

    public IReadOnlyDictionary<CriterionKind, int> CriterionImportance =>
        Weights.ToDictionary(weight => weight.Kind, weight => weight.Importance);

    public SimulationRequest ToRequest() =>
        new(Name, Description, Needs, CandidateParkIds, CriterionImportance, MandatoryCriteria);
}

public sealed record SimulationListItem(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    int CandidateCount,
    int EligibleCount,
    string? BestParkName,
    decimal? BestParkMonthlyCost,
    decimal? ConsistencyRatio);

public sealed class SimulationValidationException(IReadOnlyList<string> errors)
    : Exception(string.Join(Environment.NewLine, errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
