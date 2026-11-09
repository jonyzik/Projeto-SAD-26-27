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
        var service = new SimulationService(source, new FakeStore(), new EigenvectorAhpEngine(), new FakeUser(user));

        var outcome = await service.RunAsync(Request(source.Parks));
        var id = await service.SaveAsync(outcome);

        Assert.IsTrue(outcome.IsDemonstration);
        Assert.AreEqual(id, source.SavedId);
        Assert.AreEqual(user, source.SavedUser);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.SaveAsync(outcome with { Id = id }));
    }

    [TestMethod]
    public async Task DefaultParametersAreValidatedBeforeBeingStored()
    {
        var store = new FakeStore();
        var service = new SimulationService(new FakeSource([]), store, new EigenvectorAhpEngine(), new FakeUser(null));
        var invalid = SimulationParameters.Default with { Needs = CompanyNeeds.Default with { EmployeeCount = 0 } };

        await Assert.ThrowsExactlyAsync<SimulationValidationException>(() => service.SaveDefaultParametersAsync(invalid));
        Assert.AreEqual(SimulationParameters.Default, await service.GetDefaultParametersAsync());

        var valid = SimulationParameters.Default with { Needs = CompanyNeeds.Default with { EmployeeCount = 25 } };
        await service.SaveDefaultParametersAsync(valid);
        Assert.AreEqual(25, (await service.GetDefaultParametersAsync()).Needs.EmployeeCount);
        Assert.AreEqual(SimulationParameters.Default, await service.ResetDefaultParametersAsync());
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

    private sealed class FakeStore : ISimulationParametersStore
    {
        private SimulationParameters _value = SimulationParameters.Default;
        public Task<SimulationParameters> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(_value);
        public Task SaveAsync(SimulationParameters parameters, CancellationToken cancellationToken = default)
        {
            _value = parameters;
            return Task.CompletedTask;
        }
        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            _value = SimulationParameters.Default;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUser(Guid? id) : ICurrentUserAccessor
    {
        public Guid? CurrentUserId { get; } = id;
    }
}
