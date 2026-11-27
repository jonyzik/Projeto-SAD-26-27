using System.Windows.Input;

namespace ParkWise.App.ViewModels;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Action<Exception> _onError;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Action<Exception> onError)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _onError = onError ?? throw new ArgumentNullException(nameof(onError));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;

        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try
        {
            await _execute();
        }
        catch (Exception exception)
        {
            _onError(exception);
        }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
