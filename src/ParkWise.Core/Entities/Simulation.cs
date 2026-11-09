using ParkWise.Core.Simulations;

namespace ParkWise.Core.Entities;

public sealed class Simulation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Opcional enquanto o módulo de autenticação (dev/auth) não estiver integrado.
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public CompanyRequirements Requirements { get; set; } = new();
    public OperationalCostSettings CostSettings { get; set; } = new();
    public decimal? ConsistencyRatio { get; set; }

    public ICollection<SimulationCriterionWeight> CriterionWeights { get; set; } = new List<SimulationCriterionWeight>();
    public ICollection<AhpComparison> AhpComparisons { get; set; } = new List<AhpComparison>();
    public ICollection<SimulationResult> Results { get; set; } = new List<SimulationResult>();
}
