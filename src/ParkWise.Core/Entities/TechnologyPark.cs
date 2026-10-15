namespace ParkWise.Core.Entities;

public sealed class TechnologyPark
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MonthlyRentPerSquareMetre { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<ParkEvaluation> Evaluations { get; set; } = new List<ParkEvaluation>();
    public ICollection<SimulationResult> SimulationResults { get; set; } = new List<SimulationResult>();
}
