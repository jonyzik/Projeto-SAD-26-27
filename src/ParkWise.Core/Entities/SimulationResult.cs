namespace ParkWise.Core.Entities;

public sealed class SimulationResult
{
    public Guid Id { get; set; }
    public Guid SimulationId { get; set; }
    public Simulation Simulation { get; set; } = null!;
    public Guid TechnologyParkId { get; set; }
    public TechnologyPark TechnologyPark { get; set; } = null!;
    public int Ranking { get; set; }
    public decimal Score { get; set; }
    public decimal? EstimatedMonthlyCost { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
}
