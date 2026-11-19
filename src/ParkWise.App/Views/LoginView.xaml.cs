using System.Windows;
using System.Windows.Controls;

namespace ParkWise.App.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void PasswordInput_PasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel viewModel)
        {
            viewModel.Password = PasswordInput.Password;
        }
    }
}