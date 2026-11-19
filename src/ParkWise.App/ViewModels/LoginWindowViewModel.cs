using System.Windows.Input;
using ParkWise.App.Navigation;

namespace ParkWise.App.ViewModels;

public class LoginWindowViewModel : ViewModelBase
{
    private readonly IPageNavigation _pageNavigation;

    public LoginWindowViewModel(IPageNavigation pageNavigation)
    {
        _pageNavigation = pageNavigation;
        LoginCommand = new RelayCommand(_ => Login());
    }

    public ICommand LoginCommand { get; }

    private void Login()
    {
        // Perform login logic here (e.g., validate credentials)
        // If login is successful, navigate to the dashboard
        _pageNavigation.NavigateTo(PageKeys.Dashboard);
    }
}