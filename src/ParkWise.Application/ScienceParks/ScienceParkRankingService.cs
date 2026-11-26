using ParkWise.Core;
using ParkWise.Core.AHP;
using ParkWise.Core.Enums;

namespace ParkWise.Application.ScienceParks;

public sealed class ScienceParkRankingService
{
    public ScienceParkRankingResult Rank(
        IReadOnlyList<SciencePark> parks,
        IReadOnlyDictionary<CriterionKind, double> weights,
        string preferredCity)
    {
        ArgumentNullException.ThrowIfNull(parks);
        ArgumentNullException.ThrowIfNull(weights);

        if (string.IsNullOrWhiteSpace(preferredCity))
            throw new ArgumentException("É necessário indicar uma cidade preferida.", nameof(preferredCity));

        ValidateWeights(weights);

        var eligibleParks = new List<SciencePark>();
        var unrankedParks = new List<UnrankedSciencePark>();

        foreach (var park in parks)
        {
            ArgumentNullException.ThrowIfNull(park);

            var availableServices = park.AvailableServices;
            var missingCriteria = EvaluationCriteria.Required
                .Where(criterion =>
                    criterion.Kind != CriterionKind.CompetitiveRentPerSquareMetre &&
                    (availableServices is null || !availableServices.ContainsKey(criterion.Kind)))
                .Select(criterion => criterion.Name)
                .ToList();

            if (string.IsNullOrWhiteSpace(park.City))
                missingCriteria.Add("Localização (cidade)");

            if (park.MonthlyRentPerSquareMetre is not > 0)
                missingCriteria.Add("Preço competitivo da área alugada, por m²");

            if (missingCriteria.Count > 0)
            {
                unrankedParks.Add(new UnrankedSciencePark(
                    park,
                    $"Dados em falta ou inválidos: {string.Join(", ", missingCriteria)}."));
                continue;
            }

            eligibleParks.Add(park);
        }

        if (eligibleParks.Count < 2)
        {
            foreach (var park in eligibleParks)
            {
                unrankedParks.Add(new UnrankedSciencePark(
                    park,
                    "São necessárias pelo menos duas alternativas com dados completos para gerar um ranking."));
            }

            return new ScienceParkRankingResult([], unrankedParks.ToArray());
        }

        var rents = eligibleParks
            .Select(park => park.MonthlyRentPerSquareMetre!.Value)
            .ToArray();
        var minimumRent = rents.Min();
        var maximumRent = rents.Max();

        var orderedParks = eligibleParks
            .Select(park =>
            {
                var criterionScores = AHPCriteria.Required
                    .Select(criterion =>
                    {
                        var score = GetCriterionScore(
                            park,
                            criterion.Kind,
                            preferredCity,
                            minimumRent,
                            maximumRent);
                        var weight = weights[criterion.Kind];

                        return new ScienceParkCriterionContribution(
                            criterion.Kind,
                            criterion.Name,
                            weight,
                            score,
                            weight * score);
                    })
                    .ToArray();

                return new ScienceParkRankingEntry(
                    park,
                    0,
                    criterionScores.Sum(criterion => criterion.Contribution),
                    criterionScores);
            })
            .OrderByDescending(entry => entry.Score)
            .ThenBy(entry => entry.Park.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        var rankedParks = new List<ScienceParkRankingEntry>(orderedParks.Length);
        var currentRank = 0;
        double? previousScore = null;

        for (var index = 0; index < orderedParks.Length; index++)
        {
            var entry = orderedParks[index];
            if (previousScore is null || Math.Abs(entry.Score - previousScore.Value) > 1e-10)
                currentRank = index + 1;

            rankedParks.Add(entry with { Rank = currentRank });
            previousScore = entry.Score;
        }

        return new ScienceParkRankingResult(rankedParks.ToArray(), unrankedParks.ToArray());
    }

    private static double GetCriterionScore(
        SciencePark park,
        CriterionKind kind,
        string preferredCity,
        decimal minimumRent,
        decimal maximumRent)
    {
        if (kind == CriterionKind.GeographicLocation)
        {
            return string.Equals(
                park.City.Trim(),
                preferredCity.Trim(),
                StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;
        }

        if (kind == CriterionKind.CompetitiveRentPerSquareMetre)
        {
            if (maximumRent == minimumRent)
                return 1;

            var rent = park.MonthlyRentPerSquareMetre!.Value;
            return (double)(maximumRent - rent) / (double)(maximumRent - minimumRent);
        }

        return park.AvailableServices[kind] ? 1 : 0;
    }

    private static void ValidateWeights(IReadOnlyDictionary<CriterionKind, double> weights)
    {
        var requiredKinds = AHPCriteria.Required.Select(criterion => criterion.Kind).ToArray();
        if (weights.Count != requiredKinds.Length ||
            requiredKinds.Any(kind => !weights.ContainsKey(kind)))
        {
            throw new ArgumentException(
                "É necessário fornecer um peso AHP para cada critério obrigatório.",
                nameof(weights));
        }

        var weightSum = 0.0;
        foreach (var weight in weights.Values)
        {
            if (!double.IsFinite(weight) || weight < 0)
                throw new ArgumentOutOfRangeException(nameof(weights), "Os pesos têm de ser finitos e não negativos.");

            weightSum += weight;
        }

        if (Math.Abs(weightSum - 1) > 1e-6)
            throw new ArgumentException("A soma dos pesos AHP tem de ser igual a 1.", nameof(weights));
    }
}

public sealed record ScienceParkRankingResult(
    IReadOnlyList<ScienceParkRankingEntry> RankedParks,
    IReadOnlyList<UnrankedSciencePark> UnrankedParks);

public sealed record ScienceParkRankingEntry(
    SciencePark Park,
    int Rank,
    double Score,
    IReadOnlyList<ScienceParkCriterionContribution> Criteria);

public sealed record ScienceParkCriterionContribution(
    CriterionKind Kind,
    string Name,
    double Weight,
    double Score,
    double Contribution);

public sealed record UnrankedSciencePark(SciencePark Park, string Reason);
