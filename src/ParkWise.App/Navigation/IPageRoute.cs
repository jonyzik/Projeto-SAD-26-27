namespace ParkWise.App.Navigation;

public interface IPageRoute
{
    string Key { get; }
    string MenuLabel { get; }
    string Title { get; }
    string Subtitle { get; }
    string Icon { get; }
    bool IsVisibleInSidebar { get; }
    string? ParentMenuKey { get; }
    object CreateViewModel();
    void OnNavigated();
}
