namespace ParkWise.Core.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();
}
