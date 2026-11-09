using ParkWise.Application.Simulations;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.Tests;

internal static class SimulationTestData
{
    public static readonly CriterionKind[] Criteria =
        EvaluationCriteria.Required.Select(criterion => criterion.Kind).ToArray();

    public static CandidatePark Park(string name, decimal? rent, decimal score = 5m, string city = "Porto",
        IDictionary<CriterionKind, decimal>? overrides = null, bool omitWifi = false)
    {
        var scores = Criteria
            .Where(kind => kind != CriterionKind.CompetitiveRentPerSquareMetre)
            .Where(kind => !(omitWifi && kind == CriterionKind.Wifi))
            .ToDictionary(kind => kind, _ => score);
        if (overrides is not null)
            foreach (var (kind, value) in overrides) scores[kind] = value;
        return new CandidatePark(Guid.NewGuid(), name, city, rent, scores);
    }

    public static SimulationRequest Request(IEnumerable<CandidatePark> parks,
        IReadOnlyList<CriterionComparison>? comparisons = null,
        SimulationParameters? parameters = null,
        IReadOnlyCollection<CriterionKind>? mandatory = null,
        string name = "Teste") =>
        new(name, null, parameters ?? SimulationParameters.Default,
            parks.Select(park => park.Id).ToArray(),
            comparisons ?? [],
            mandatory ?? []);
}
