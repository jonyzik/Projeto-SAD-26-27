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
        NavigateCommand = new RelayCommand(Navigate);
    }

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
        var destination = parameter as string;
        switch (destination)
        {
            case "Dashboard":
                CurrentTitle = "Dashboard";
                CurrentPage = _dashboard;
                break;
            case "Parques":
                ShowPlaceholder("Parques Tecnológicos", "O registo, a importação e a pesquisa de parques serão implementados numa fase seguinte.");
                break;
            case "Simulações":
                ShowPlaceholder("Simulações", "A configuração de preferências, o método AHP e o cálculo de custos serão implementados numa fase seguinte.");
                break;
            case "Histórico":
                ShowPlaceholder("Histórico", "O histórico de simulações está previsto para uma fase futura.");
                break;
            case "Definições":
                ShowPlaceholder("Definições", "As definições e a gestão de utilizadores estão previstas para uma fase futura.");
                break;
        }
    }

    private void ShowPlaceholder(string title, string description)
    {
        CurrentTitle = title;
        CurrentPage = new PlaceholderViewModel(title, description);
    }
}
