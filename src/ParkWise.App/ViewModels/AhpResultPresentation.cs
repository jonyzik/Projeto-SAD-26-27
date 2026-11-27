namespace ParkWise.App.ViewModels;

public enum AhpResultDisplayState
{
    AwaitingEngine,
    Available,
    Inconsistent,
    Error
}

/// <summary>Dados de apresentação dos resultados AHP.</summary>
public sealed record AhpResultPresentation(
    AhpResultDisplayState State,
    IReadOnlyList<AhpWeightDisplay> Weights,
    string ConsistencyMessage,
    string ErrorMessage)
{
    public static AhpResultPresentation Awaiting { get; } = new(
        AhpResultDisplayState.AwaitingEngine,
        [],
        "A consistência será calculada quando as comparações estiverem concluídas.",
        "Ainda não existem erros para apresentar.");
}

public sealed record AhpWeightDisplay(string CriterionName, string ValueText);
public sealed record AhpReviewRow(string LeftName, string RightName, string Choice);
public sealed record ParkRankingDisplay(
    int Rank,
    string Name,
    string City,
    string ScoreText,
    IReadOnlyList<ParkCriterionContributionDisplay> Criteria);
public sealed record ParkCriterionContributionDisplay(
    string Name,
    string DetailText);
public sealed record UnrankedParkDisplay(string Name, string Reason);
