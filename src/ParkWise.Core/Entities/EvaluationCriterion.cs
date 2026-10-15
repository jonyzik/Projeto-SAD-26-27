using ParkWise.Core.Enums;

namespace ParkWise.Core.Entities;

public sealed class EvaluationCriterion
{
    public int Id { get; set; }
    public CriterionKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ParkEvaluation> ParkEvaluations { get; set; } = new List<ParkEvaluation>();
    public ICollection<SimulationCriterionWeight> SimulationWeights { get; set; } = new List<SimulationCriterionWeight>();
}
