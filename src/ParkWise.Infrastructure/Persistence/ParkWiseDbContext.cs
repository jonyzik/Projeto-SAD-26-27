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
            entity.Property(x => x.RequiredAreaSquareMetres).HasPrecision(12, 2);
            entity.HasOne(x => x.User).WithMany(x => x.Simulations)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
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
            entity.Property(x => x.Strengths).HasMaxLength(2000);
            entity.Property(x => x.Weaknesses).HasMaxLength(2000);
            entity.HasIndex(x => new { x.SimulationId, x.TechnologyParkId }).IsUnique();
        });
    }
}
