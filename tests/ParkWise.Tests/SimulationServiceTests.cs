using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.Abstractions;
using ParkWise.Application.Simulations;
using static ParkWise.Tests.SimulationTestData;

namespace ParkWise.Tests;

[TestClass]
public sealed class SimulationServiceTests
{
    [TestMethod]
    public async Task RunsWithTheCatalogAndSavesWithTheCurrentUser()
    {
        var user = Guid.NewGuid();
        var source = new FakeSource([Park("A", 10m), Park("B", 12m)]);
        var service = new SimulationService(source, new EigenvectorAhpEngine(), new FakeUser(user));

        var outcome = await service.RunAsync(Request(source.Parks));
        var id = await service.SaveAsync(outcome);

        Assert.IsTrue(outcome.IsDemonstration);
        Assert.AreEqual(id, source.SavedId);
        Assert.AreEqual(user, source.SavedUser);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.SaveAsync(outcome with { Id = id }));
    }

    private sealed class FakeSource(IReadOnlyList<CandidatePark> parks) : ISimulationDataSource
    {
        public IReadOnlyList<CandidatePark> Parks { get; } = parks;
        public Guid? SavedId { get; private set; }
        public Guid? SavedUser { get; private set; }

        public Task<CandidateParkCatalog> LoadCandidateParksAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new CandidateParkCatalog(Parks, true));

        public Task<Guid> SaveAsync(SimulationOutcome outcome, Guid? userId, CancellationToken cancellationToken = default)
        {
            SavedId = Guid.NewGuid();
            SavedUser = userId;
            return Task.FromResult(SavedId.Value);
        }

        public Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SimulationListItem>>([]);
        public Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<SimulationOutcome?>(null);
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FakeUser(Guid? id) : ICurrentUserAccessor
    {
        public Guid? CurrentUserId { get; } = id;
    }
}
