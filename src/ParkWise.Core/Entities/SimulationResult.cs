namespace ParkWise.Core.Entities;

public sealed class SimulationResult
{
    public Guid Id { get; set; }
    public Guid SimulationId { get; set; }
    public Simulation Simulation { get; set; } = null!;
    public Guid TechnologyParkId { get; set; }
    public TechnologyPark TechnologyPark { get; set; } = null!;

    /// <summary>Posição no ranking; 0 quando o parque não é elegível.</summary>
    public int Ranking { get; set; }
    public decimal Score { get; set; }
    public bool IsEligible { get; set; }
    public string? ExclusionReason { get; set; }
    public decimal? EstimatedMonthlyCost { get; set; }
    public decimal? EstimatedSetupCost { get; set; }
    public decimal? EstimatedContractCost { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
}
