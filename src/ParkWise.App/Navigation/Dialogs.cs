using System.Windows;

namespace ParkWise.App.Navigation;

public static class Dialogs
{
    public static bool Confirm(string message) =>
        MessageBox.Show(message, "ParkWise", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
        == MessageBoxResult.Yes;
}
