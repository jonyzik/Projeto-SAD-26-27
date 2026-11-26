using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using ParkWise.App.ViewModels;

namespace ParkWise.App.Views;

public partial class ScienceParkSearchView : UserControl
{
    private static readonly JsonSerializerOptions MapJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SemaphoreSlim _mapRefreshGate = new(1, 1);
    private ScienceParkSearchViewModel? _viewModel;
    private bool _virtualHostMapped;
    private bool _navigationStarted;
    private bool _mapPageLoaded;
    private bool _mapInitialized;

    public ScienceParkSearchView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsLoaded)
            AttachViewModel(e.NewValue as ScienceParkSearchViewModel);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AttachViewModel(DataContext as ScienceParkSearchViewModel);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        AttachViewModel(null);
    }

    private void AttachViewModel(ScienceParkSearchViewModel? viewModel)
    {
        if (ReferenceEquals(_viewModel, viewModel))
            return;

        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = viewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        _ = RefreshMapAsync();
    }

    private void OnViewModelPropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScienceParkSearchViewModel.Results))
        {
            _ = RefreshMapAsync();
        }
    }

    private async Task RefreshMapAsync()
    {
        await _mapRefreshGate.WaitAsync();
        try
        {
            var locations = _viewModel?.MapPoints ?? [];
            if (locations.Count == 0)
            {
                MapBrowser.Visibility = Visibility.Collapsed;
                MapSetupPanel.Visibility = Visibility.Collapsed;
                MapEmptyPanel.Visibility = Visibility.Visible;
                return;
            }

            MapSetupPanel.Visibility = Visibility.Collapsed;
            MapEmptyPanel.Visibility = Visibility.Collapsed;
            MapBrowser.Visibility = Visibility.Visible;

            await MapBrowser.EnsureCoreWebView2Async();
            var core = MapBrowser.CoreWebView2;

            if (!_navigationStarted)
            {
                if (!_virtualHostMapped)
                {
                    core.SetVirtualHostNameToFolderMapping(
                        "parkwise.local",
                        AppContext.BaseDirectory,
                        CoreWebView2HostResourceAccessKind.DenyCors);
                    _virtualHostMapped = true;
                }

                core.NavigationCompleted -= OnMapNavigationCompleted;
                core.NavigationCompleted += OnMapNavigationCompleted;
                _navigationStarted = true;
                MapSetupTitle.Text = "A preparar o mapa";
                MapSetupMessage.Text = "O mapa OpenStreetMap está a iniciar.";
                MapSetupPanel.Visibility = Visibility.Visible;
                core.Navigate("https://parkwise.local/Maps/OpenStreetMap.html");
                return;
            }

            if (!_mapPageLoaded)
                return;

            var serializedLocations = JsonSerializer.Serialize(locations, MapJsonOptions);
            var shouldInitialize = !_mapInitialized;
            var script = shouldInitialize
                ? $"window.parkwiseMap.initialize({serializedLocations});"
                : $"window.parkwiseMap.setLocations({serializedLocations});";

            var result = await core.ExecuteScriptAsync(script);
            _mapInitialized = string.Equals(result, "true", StringComparison.Ordinal);
        }
        catch (Exception exception)
        {
            _navigationStarted = false;
            _mapPageLoaded = false;
            _mapInitialized = false;
            ShowMapMessage(
                "Não foi possível iniciar o mapa",
                $"Verifique o runtime WebView2 e a ligação à internet. {exception.Message}");
        }
        finally
        {
            _mapRefreshGate.Release();
        }
    }

    private async void OnMapNavigationCompleted(
        object? sender,
        CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            _navigationStarted = false;
            _mapPageLoaded = false;
            _mapInitialized = false;
            ShowMapMessage(
                "Não foi possível abrir o mapa",
                $"O WebView2 não conseguiu carregar a página local do mapa (erro {e.WebErrorStatus}).");
            return;
        }

        _mapPageLoaded = true;
        await RefreshMapAsync();
    }

    private void ShowMapMessage(string title, string message)
    {
        MapBrowser.Visibility = Visibility.Collapsed;
        MapEmptyPanel.Visibility = Visibility.Collapsed;
        MapSetupTitle.Text = title;
        MapSetupMessage.Text = message;
        MapSetupPanel.Visibility = Visibility.Visible;
    }
}
