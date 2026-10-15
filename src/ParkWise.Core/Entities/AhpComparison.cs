namespace ParkWise.Core.Entities;

public sealed class AhpComparison
{
    public Guid Id { get; set; }
    public Guid SimulationId { get; set; }
    public Simulation Simulation { get; set; } = null!;
    public int LeftCriterionId { get; set; }
    public EvaluationCriterion LeftCriterion { get; set; } = null!;
    public int RightCriterionId { get; set; }
    public EvaluationCriterion RightCriterion { get; set; } = null!;
    public decimal PreferenceValue { get; set; }
}
