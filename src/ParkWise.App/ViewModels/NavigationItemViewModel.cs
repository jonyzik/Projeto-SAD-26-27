namespace ParkWise.App.ViewModels;

public sealed class NavigationItemViewModel(string key, string label, string icon) : ViewModelBase
{
    private bool _isActive;

    public string Key { get; } = key;
    public string Label { get; } = label;
    public string Icon { get; } = icon;

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}
