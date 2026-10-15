using System.Windows;
using ParkWise.App.ViewModels;

namespace ParkWise.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
