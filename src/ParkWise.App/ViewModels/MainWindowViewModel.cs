using System.Windows.Input;
using ParkWise.App.Navigation;

namespace ParkWise.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IReadOnlyDictionary<string, IPageRoute> _routes;
    private object _currentPage;
    private string _currentKey = PageKeys.Dashboard;
    private string _currentTitle;
    private string _currentSubtitle;

    public MainWindowViewModel(IEnumerable<IPageRoute> routes)
    {
        var registeredRoutes = routes.ToArray();
        _routes = registeredRoutes.ToDictionary(route => route.Key, StringComparer.Ordinal);
        var dashboard = _routes[PageKeys.Dashboard];
        _currentPage = dashboard.CreateViewModel();
        _currentTitle = dashboard.Title;
        _currentSubtitle = dashboard.Subtitle;
        NavigationItems = registeredRoutes
            .Where(route => route.IsVisibleInSidebar)
            .Select(route => new NavigationItemViewModel(route.Key, route.MenuLabel, route.Icon))
            .ToArray();
        SetActiveItem(PageKeys.Dashboard);
        NavigateCommand = new RelayCommand(Navigate);
    }

    public IReadOnlyList<NavigationItemViewModel> NavigationItems { get; }

    public object CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public string CurrentTitle
    {
        get => _currentTitle;
        private set => SetProperty(ref _currentTitle, value);
    }

    public string CurrentSubtitle
    {
        get => _currentSubtitle;
        private set => SetProperty(ref _currentSubtitle, value);
    }

    public ICommand NavigateCommand { get; }

    public void ActivateCurrentPage() => _routes[_currentKey].OnNavigated();

    private void Navigate(object? parameter)
    {
        var key = parameter is NavigationItemViewModel item ? item.Key : parameter as string;
        if (key is null || !_routes.TryGetValue(key, out var route)) return;

        _currentKey = key;
        CurrentTitle = route.Title;
        CurrentSubtitle = route.Subtitle;
        CurrentPage = route.CreateViewModel();
        SetActiveItem(route.ParentMenuKey ?? route.Key);
        route.OnNavigated();
    }

    private void SetActiveItem(string key)
    {
        foreach (var item in NavigationItems)
            item.IsActive = item.Key == key;
    }
}
