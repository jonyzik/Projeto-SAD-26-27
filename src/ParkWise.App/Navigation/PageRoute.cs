namespace ParkWise.App.Navigation;

public sealed record PageRoute(
    string Key,
    string MenuLabel,
    string Title,
    string Subtitle,
    string Icon,
    Func<object> ViewModelFactory,
    Action? Activated = null,
    bool IsVisibleInSidebar = true,
    string? ParentMenuKey = null) : IPageRoute
{
    public object CreateViewModel() => ViewModelFactory();
    public void OnNavigated() => Activated?.Invoke();
}
