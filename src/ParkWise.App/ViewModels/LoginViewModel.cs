
using System.Windows.Input;

namespace ParkWise.App.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private string _username = string.Empty;

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(_ => Login());
    }

    public event Action? LoginSucceeded;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }

    private void Login()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Preenche o email e a palavra-passe.";
            return;
        }

        const string demoEmail = "teste@parkwise.pt";
        const string demoPassword = "ParkWise123!";

        bool validEmail = string.Equals(
            Email.Trim(),
            demoEmail,
            StringComparison.OrdinalIgnoreCase);

        bool validPassword = Password == demoPassword;

        if (!validEmail || !validPassword)
        {
            ErrorMessage = "Email ou palavra-passe incorretos.";
            return;
        }

        LoginSucceeded?.Invoke();
    }
}
