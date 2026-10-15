namespace ParkWise.Core.Entities;

public sealed class Simulation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public decimal? RequiredAreaSquareMetres { get; set; }
    public ICollection<SimulationCriterionWeight> CriterionWeights { get; set; } = new List<SimulationCriterionWeight>();
    public ICollection<AhpComparison> AhpComparisons { get; set; } = new List<AhpComparison>();
    public ICollection<SimulationResult> Results { get; set; } = new List<SimulationResult>();
}
