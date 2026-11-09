using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

/// <summary>Necessidades da empresa que serve de base à simulação.</summary>
public sealed record CompanyNeeds(
    decimal RequiredAreaSquareMetres,
    int EmployeeCount,
    int ContractDurationMonths,
    decimal? MaxMonthlyBudget,
    string? PreferredCity)
{
    public static CompanyNeeds Default { get; } = new(120m, 10, 36, null, null);
}

/// <summary>Parâmetros dos custos operacionais. Valores em euros por mês, salvo o custo de instalação.</summary>
public sealed record CostParameters(
    decimal ServiceChargePerSquareMetre,
    decimal UtilitiesPerSquareMetre,
    decimal ServicesPerEmployee,
    decimal ParkingSpacesRatio,
    decimal ParkingCostPerSpace,
    decimal SetupCostPerSquareMetre,
    decimal ContingencyPercentage)
{
    public static CostParameters Default { get; } = new(
        ServiceChargePerSquareMetre: 2.50m,
        UtilitiesPerSquareMetre: 1.80m,
        ServicesPerEmployee: 35m,
        ParkingSpacesRatio: 0.50m,
        ParkingCostPerSpace: 40m,
        SetupCostPerSquareMetre: 25m,
        ContingencyPercentage: 5m);
}

public sealed record SimulationParameters(CompanyNeeds Needs, CostParameters Costs)
{
    public static SimulationParameters Default { get; } = new(CompanyNeeds.Default, CostParameters.Default);
}

/// <summary>
/// Comparação par a par entre dois critérios na escala de Saaty.
/// <see cref="Value"/> &gt; 1 significa que <see cref="Left"/> é mais importante;
/// um valor entre 0 e 1 (por exemplo 1/5) significa que <see cref="Right"/> é mais importante.
/// </summary>
public sealed record CriterionComparison(CriterionKind Left, CriterionKind Right, decimal Value);

public sealed record SimulationRequest(
    string Name,
    string? Description,
    SimulationParameters Parameters,
    IReadOnlyList<Guid> CandidateParkIds,
    IReadOnlyList<CriterionComparison> Comparisons,
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

public sealed record OperationalCostBreakdown(
    decimal Rent,
    decimal ServiceCharges,
    decimal Utilities,
    decimal EmployeeServices,
    int ParkingSpaces,
    decimal Parking,
    decimal Contingency,
    decimal MonthlyTotal,
    decimal AnnualTotal,
    decimal SetupCost,
    decimal ContractTotal);

public sealed record CriterionWeight(CriterionKind Kind, string Name, decimal Weight, bool IsMandatory);

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
    decimal? MonthlyCost,
    decimal? SetupCost,
    decimal? ContractCost,
    OperationalCostBreakdown? CostBreakdown,
    IReadOnlyList<CriterionContribution> Contributions,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses);

public sealed record SimulationOutcome(
    Guid? Id,
    string Name,
    string? Description,
    DateTime ExecutedAtUtc,
    SimulationParameters Parameters,
    IReadOnlyList<CriterionWeight> Weights,
    ConsistencyCheck Consistency,
    IReadOnlyList<CriterionComparison> Comparisons,
    IReadOnlyList<ParkSimulationResult> Results,
    bool IsDemonstration)
{
    public IReadOnlyList<ParkSimulationResult> RankedResults =>
        Results.Where(result => result.Ranking is not null).OrderBy(result => result.Ranking).ToArray();

    public ParkSimulationResult? BestResult => RankedResults.FirstOrDefault();

    public IReadOnlyList<Guid> CandidateParkIds => Results.Select(result => result.ParkId).ToArray();

    public IReadOnlyCollection<CriterionKind> MandatoryCriteria =>
        Weights.Where(weight => weight.IsMandatory).Select(weight => weight.Kind).ToArray();

    public SimulationRequest ToRequest() =>
        new(Name, Description, Parameters, CandidateParkIds, Comparisons, MandatoryCriteria);
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
