using System.Windows.Input;

namespace ParkWise.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly DashboardViewModel _dashboard;
    private object _currentPage;
    private string _currentTitle = "Dashboard";

    public MainWindowViewModel(DashboardViewModel dashboard)
    {
        _dashboard = dashboard;
        _currentPage = dashboard;
        NavigationItems =
        [
            new("Dashboard", "Dashboard", "\uE80F"),
            new("Parques", "Parques Tecnológicos", "\uE80D"),
            new("Simulações", "Simulações", "\uE7C3"),
            new("Histórico", "Histórico", "\uE81C"),
            new("Definições", "Definições", "\uE713")
        ];
        NavigationItems[0].IsActive = true;
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

    public ICommand NavigateCommand { get; }

    private void Navigate(object? parameter)
    {
        var destination = parameter is NavigationItemViewModel item ? item.Key : parameter as string;
        if (destination is null ||
            (destination != "NovaSimulação" && !NavigationItems.Any(entry => entry.Key == destination))) return;

        var activeKey = destination == "NovaSimulação" ? "Simulações" : destination;
        foreach (var entry in NavigationItems)
            entry.IsActive = entry.Key == activeKey;

        switch (destination)
        {
            case "Dashboard":
                CurrentTitle = "Dashboard";
                CurrentPage = _dashboard;
                _dashboard.RefreshCommand.Execute(null);
                break;
            case "Parques":
                ShowPlaceholder("Parques Tecnológicos", "O registo, a importação e a pesquisa de parques estão em desenvolvimento.");
                break;
            case "Simulações":
                ShowPlaceholder("Simulações", "A criação de simulações, o método AHP e o cálculo de custos estão em desenvolvimento.");
                break;
            case "NovaSimulação":
                ShowPlaceholder("Nova simulação", "A criação de uma nova simulação está em desenvolvimento.");
                break;
            case "Histórico":
                ShowPlaceholder("Histórico", "A consulta do histórico de simulações está em desenvolvimento.");
                break;
            case "Definições":
                ShowPlaceholder("Definições", "As definições e a gestão de utilizadores estão em desenvolvimento.");
                break;
        }
    }

    private void ShowPlaceholder(string title, string description)
    {
        CurrentTitle = title;
        CurrentPage = new PlaceholderViewModel(title, description);
    }
}
