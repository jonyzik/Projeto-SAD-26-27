using Microsoft.EntityFrameworkCore;
using ParkWise.Application.Abstractions;
using ParkWise.Core.Entities;

namespace ParkWise.Infrastructure.Persistence;

public sealed class ParkWiseDbContext(DbContextOptions<ParkWiseDbContext> options)
    : DbContext(options), IParkWiseDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TechnologyPark> TechnologyParks => Set<TechnologyPark>();
    public DbSet<EvaluationCriterion> EvaluationCriteria => Set<EvaluationCriterion>();
    public DbSet<ParkEvaluation> ParkEvaluations => Set<ParkEvaluation>();
    public DbSet<Simulation> Simulations => Set<Simulation>();
    public DbSet<SimulationCriterionWeight> SimulationCriterionWeights => Set<SimulationCriterionWeight>();
    public DbSet<AhpComparison> AhpComparisons => Set<AhpComparison>();
    public DbSet<SimulationResult> SimulationResults => Set<SimulationResult>();

    IQueryable<TechnologyPark> IParkWiseDbContext.TechnologyParks => TechnologyParks;
    IQueryable<EvaluationCriterion> IParkWiseDbContext.EvaluationCriteria => EvaluationCriteria;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<TechnologyPark>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.City).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.MonthlyRentPerSquareMetre).HasPrecision(12, 2);
        });

        modelBuilder.Entity<EvaluationCriterion>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Kind).HasConversion<int>();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => x.Kind).IsUnique();
            entity.HasData(ParkWise.Core.EvaluationCriteria.Required.Select(x => new EvaluationCriterion
            {
                Id = (int)x.Kind,
                Kind = x.Kind,
                Name = x.Name
            }));
        });

        modelBuilder.Entity<ParkEvaluation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Score).HasPrecision(9, 4);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TechnologyParkId, x.EvaluationCriterionId }).IsUnique();
        });

        modelBuilder.Entity<Simulation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.ConsistencyRatio).HasPrecision(9, 6);
            entity.HasIndex(x => x.CreatedAtUtc);
            // Utilizador opcional até à integração do módulo de autenticação.
            entity.HasOne(x => x.User).WithMany(x => x.Simulations)
                .HasForeignKey(x => x.UserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            // Necessidades da empresa e parâmetros de custo ficam em colunas da própria tabela Simulations.
            entity.OwnsOne(x => x.Requirements, owned =>
            {
                owned.Property(p => p.RequiredAreaSquareMetres).HasColumnName("RequiredAreaSquareMetres").HasPrecision(12, 2);
                owned.Property(p => p.EmployeeCount).HasColumnName("EmployeeCount");
                owned.Property(p => p.ContractDurationMonths).HasColumnName("ContractDurationMonths");
                owned.Property(p => p.MaxMonthlyBudget).HasColumnName("MaxMonthlyBudget").HasPrecision(14, 2);
                owned.Property(p => p.PreferredCity).HasColumnName("PreferredCity").HasMaxLength(120);
            });
            entity.Navigation(x => x.Requirements).IsRequired();

            entity.OwnsOne(x => x.CostSettings, owned =>
            {
                owned.Property(p => p.ServiceChargePerSquareMetre).HasColumnName("ServiceChargePerSquareMetre").HasPrecision(12, 2);
                owned.Property(p => p.UtilitiesPerSquareMetre).HasColumnName("UtilitiesPerSquareMetre").HasPrecision(12, 2);
                owned.Property(p => p.ServicesPerEmployee).HasColumnName("ServicesPerEmployee").HasPrecision(12, 2);
                owned.Property(p => p.ParkingSpacesRatio).HasColumnName("ParkingSpacesRatio").HasPrecision(5, 4);
                owned.Property(p => p.ParkingCostPerSpace).HasColumnName("ParkingCostPerSpace").HasPrecision(12, 2);
                owned.Property(p => p.SetupCostPerSquareMetre).HasColumnName("SetupCostPerSquareMetre").HasPrecision(12, 2);
                owned.Property(p => p.ContingencyPercentage).HasColumnName("ContingencyPercentage").HasPrecision(5, 2);
            });
            entity.Navigation(x => x.CostSettings).IsRequired();
        });

        modelBuilder.Entity<SimulationCriterionWeight>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Weight).HasPrecision(12, 8);
            entity.HasIndex(x => new { x.SimulationId, x.EvaluationCriterionId }).IsUnique();
        });

        modelBuilder.Entity<AhpComparison>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PreferenceValue).HasPrecision(12, 8);
            entity.HasOne(x => x.LeftCriterion).WithMany().HasForeignKey(x => x.LeftCriterionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RightCriterion).WithMany().HasForeignKey(x => x.RightCriterionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.SimulationId, x.LeftCriterionId, x.RightCriterionId }).IsUnique();
        });

        modelBuilder.Entity<SimulationResult>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Score).HasPrecision(12, 8);
            entity.Property(x => x.EstimatedMonthlyCost).HasPrecision(14, 2);
            entity.Property(x => x.EstimatedSetupCost).HasPrecision(14, 2);
            entity.Property(x => x.EstimatedContractCost).HasPrecision(16, 2);
            entity.Property(x => x.ExclusionReason).HasMaxLength(2000);
            entity.Property(x => x.Strengths).HasMaxLength(2000);
            entity.Property(x => x.Weaknesses).HasMaxLength(2000);
            entity.HasIndex(x => new { x.SimulationId, x.TechnologyParkId }).IsUnique();
        });
    }
}
