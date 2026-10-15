namespace ParkWise.Core.Entities;

public sealed class ParkEvaluation
{
    public Guid Id { get; set; }
    public Guid TechnologyParkId { get; set; }
    public TechnologyPark TechnologyPark { get; set; } = null!;
    public int EvaluationCriterionId { get; set; }
    public EvaluationCriterion EvaluationCriterion { get; set; } = null!;
    public decimal Score { get; set; }
    public string? Notes { get; set; }
}
