namespace ParkWise.App.ViewModels;

public enum AhpResultDisplayState
{
    AwaitingEngine,
    Available,
    Inconsistent,
    Error
}

/// <summary>Dados de apresentação a preencher quando o motor AHP estiver integrado.</summary>
public sealed record AhpResultPresentation(
    AhpResultDisplayState State,
    IReadOnlyList<AhpWeightDisplay> Weights,
    string ConsistencyMessage,
    string ErrorMessage)
{
    public static AhpResultPresentation Awaiting { get; } = new(
        AhpResultDisplayState.AwaitingEngine,
        [],
        "A consistência será verificada pelo motor AHP.",
        "Sem erros a apresentar enquanto o motor AHP não for executado.");
}

public sealed record AhpWeightDisplay(string CriterionName, string ValueText);
public sealed record AhpReviewRow(string LeftName, string RightName, string Choice);
public sealed class SaatyScaleOption(int intensity, string label) : ViewModelBase
{
    private bool _isSelected;

    public int Intensity { get; } = intensity;
    public string Label { get; } = label;
    public bool IsSelected
    {
        get => _isSelected;
        private set => SetProperty(ref _isSelected, value);
    }

    public void SetSelected(bool selected) => IsSelected = selected;
}
