using ParkWise.Application.Simulations;
using ParkWise.Core.Enums;

namespace ParkWise.Infrastructure.Simulations;

/// <summary>
/// Fonte de demonstração usada quando não existe ligação à base de dados. Os parques são fictícios
/// e as simulações guardadas ficam apenas em memória até a aplicação ser fechada.
/// </summary>
public sealed class DemoSimulationDataSource : ISimulationDataSource
{
    private readonly object _gate = new();
    private readonly List<SimulationOutcome> _saved = [];
    private readonly IReadOnlyList<CandidatePark> _parks =
    [
        Park("10000000-0000-0000-0000-000000000001", "Parque Fictício A", "Localidade fictícia", 8.50m, [8, 9, 6, 7, 5, 6, 4, 8]),
        Park("10000000-0000-0000-0000-000000000002", "Parque Fictício B", "Localidade fictícia", 14.00m, [9, 10, 9, 8, 9, 8, 7, 9]),
        Park("10000000-0000-0000-0000-000000000003", "Parque Fictício C", "Localidade fictícia", 22.00m, [10, 10, 10, 9, 10, 9, 9, 10]),
        Park("10000000-0000-0000-0000-000000000004", "Parque Fictício D", "Localidade fictícia", null, [6, 7, 5, 9, 3, 5, 6, 7]),
        Park("10000000-0000-0000-0000-000000000005", "Parque Fictício E", "Outra localidade fictícia", 11.00m, [5, 8, 7, 0, 8, 4, 5, 6]),
        Park("10000000-0000-0000-0000-000000000006", "Parque Fictício F", "Outra localidade fictícia", 17.50m, [7, 9, 8, 9, 6, 9, 8, 7])
    ];

    public Task<CandidateParkCatalog> LoadCandidateParksAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new CandidateParkCatalog(_parks, IsDemonstration: true));
    }

    public Task<Guid> SaveAsync(SimulationOutcome outcome, Guid? userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var id = Guid.NewGuid();
        lock (_gate) _saved.Add(outcome with { Id = id });
        return Task.FromResult(id);
    }

    public Task<IReadOnlyList<SimulationListItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            IReadOnlyList<SimulationListItem> items = _saved
                .OrderByDescending(outcome => outcome.ExecutedAtUtc)
                .Select(outcome => new SimulationListItem(
                    outcome.Id!.Value, outcome.Name, outcome.ExecutedAtUtc,
                    outcome.Results.Count, outcome.RankedResults.Count,
                    outcome.BestResult?.ParkName, outcome.BestResult?.MonthlyCost,
                    outcome.Consistency.ConsistencyRatio))
                .ToArray();
            return Task.FromResult(items);
        }
    }

    public Task<SimulationOutcome?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate) return Task.FromResult(_saved.FirstOrDefault(outcome => outcome.Id == id));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate) return Task.FromResult(_saved.RemoveAll(outcome => outcome.Id == id) > 0);
    }

    // As pontuações seguem a ordem dos oito critérios qualitativos do catálogo; o preço é o nono critério.
    private static CandidatePark Park(string id, string name, string city, decimal? rent, int[] scores)
    {
        var kinds = Enum.GetValues<CriterionKind>()
            .Where(kind => kind != CriterionKind.CompetitiveRentPerSquareMetre)
            .ToArray();
        return new CandidatePark(Guid.Parse(id), name, city, rent,
            kinds.Select((kind, i) => (kind, score: (decimal)scores[i])).ToDictionary(x => x.kind, x => x.score));
    }
}
