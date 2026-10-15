namespace ParkWise.App.ViewModels;

public sealed record PlaceholderViewModel(
    string Title,
    string Description,
    IReadOnlyList<string> PlannedCapabilities);
