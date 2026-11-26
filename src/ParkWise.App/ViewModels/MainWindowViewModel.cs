
using System.Windows.Input;
using ParkWise.App.Navigation;

namespace ParkWise.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IPageNavigation
{
    private readonly IReadOnlyDictionary<string, IPageRoute> _routes;
    private object _currentPage;
    private string _currentKey = PageKeys.Login;
    private string _currentTitle;
    private string _currentSubtitle;
    private bool _isLoginPage = true;

    public MainWindowViewModel(IEnumerable<IPageRoute> routes)
    {
        var registeredRoutes = routes.ToArray();

        _routes = registeredRoutes.ToDictionary(
            route => route.Key,
            StringComparer.Ordinal);

        var login = _routes[PageKeys.Login];

        _currentPage = login.CreateViewModel();
        _currentTitle = login.Title;
        _currentSubtitle = login.Subtitle;

        // Quando o login for válido, abrir o Dashboard.
        if (_currentPage is LoginViewModel loginViewModel)
        {
            loginViewModel.LoginSucceeded +=
                () => NavigateTo(PageKeys.Dashboard);
        }

        NavigationItems = registeredRoutes
            .Where(route => route.IsVisibleInSidebar)
            .Select(route => new NavigationItemViewModel(
                route.Key,
                route.MenuLabel,
                route.Icon))
            .ToArray();

        SetActiveItem(PageKeys.Dashboard);

        NavigateCommand = new RelayCommand(parameter =>
        {
            var key = parameter is NavigationItemViewModel item
                ? item.Key
                : parameter as string;

            if (key is not null)
                NavigateTo(key);
        });
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

    public bool IsLoginPage
    {
        get => _isLoginPage;
        private set => SetProperty(ref _isLoginPage, value);
    }

    public ICommand NavigateCommand { get; }

    public void ActivateCurrentPage()
    {
        _routes[_currentKey].OnNavigated();
    }

    public void NavigateTo(string key)
    {
        if (!_routes.TryGetValue(key, out var route))
            return;

        _currentKey = key;
        IsLoginPage = key == PageKeys.Login;

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
