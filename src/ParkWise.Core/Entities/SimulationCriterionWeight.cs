namespace ParkWise.Core.Entities;

public sealed class SimulationCriterionWeight
{
    public Guid Id { get; set; }
    public Guid SimulationId { get; set; }
    public Simulation Simulation { get; set; } = null!;
    public int EvaluationCriterionId { get; set; }
    public EvaluationCriterion EvaluationCriterion { get; set; } = null!;
    public decimal Weight { get; set; }

    /// <summary>Indica se o parque tem obrigatoriamente de oferecer este critério.</summary>
    public bool IsMandatory { get; set; }
}
