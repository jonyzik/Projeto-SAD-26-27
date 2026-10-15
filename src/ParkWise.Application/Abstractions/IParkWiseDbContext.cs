using ParkWise.Core.Entities;

namespace ParkWise.Application.Abstractions;

public interface IParkWiseDbContext
{
    IQueryable<TechnologyPark> TechnologyParks { get; }
    IQueryable<EvaluationCriterion> EvaluationCriteria { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
