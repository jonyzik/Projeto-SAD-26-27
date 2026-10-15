using System.Globalization;
using System.Windows.Input;
using ParkWise.App.Navigation;
using ParkWise.Application.ScienceParks;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.App.ViewModels;

public sealed class ScienceParkSearchViewModel : ViewModelBase
{
    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-PT");
    private readonly IScienceParkCatalog _catalog;
    private readonly ScienceParkSelection _selection;
    private readonly ScienceParkPageState _pageState;
    private readonly IPageNavigation _navigation;
    private IReadOnlyList<SciencePark> _allParks = [];
    private IReadOnlyList<ScienceParkCardViewModel> _results = [];
    private string _nameQuery = string.Empty;
    private string _locationQuery = string.Empty;
    private string _minimumPrice = string.Empty;
    private string _maximumPrice = string.Empty;
    private string _statusMessage = "A carregar parques...";
    private string _filterMessage = string.Empty;

    public ScienceParkSearchViewModel(
        IScienceParkCatalog catalog,
        ScienceParkSelection selection,
        ScienceParkPageState pageState,
        IPageNavigation navigation)
    {
        _catalog = catalog;
        _selection = selection;
        _pageState = pageState;
        _navigation = navigation;
        ServiceFilters = EvaluationCriteria.Required
            .Where(criterion => criterion.Kind != CriterionKind.CompetitiveRentPerSquareMetre)
            .Select(criterion => new ServiceFilterViewModel(criterion.Kind, criterion.Name, ApplyFilters))
            .ToArray();
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
        ShowDetailsCommand = new RelayCommand(parameter => ShowDetails(parameter as ScienceParkCardViewModel));
        OpenComparisonCommand = new RelayCommand(_ => _navigation.NavigateTo(PageKeys.ScienceParkComparison));
        _selection.Changed += (_, _) => OnPropertyChanged(nameof(SelectedCountText));
    }

    public IReadOnlyList<ServiceFilterViewModel> ServiceFilters { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ShowDetailsCommand { get; }
    public ICommand OpenComparisonCommand { get; }

    public string NameQuery
    {
        get => _nameQuery;
        set { if (SetProperty(ref _nameQuery, value)) ApplyFilters(); }
    }

    public string LocationQuery
    {
        get => _locationQuery;
        set { if (SetProperty(ref _locationQuery, value)) ApplyFilters(); }
    }

    public string MinimumPrice
    {
        get => _minimumPrice;
        set { if (SetProperty(ref _minimumPrice, value)) ApplyFilters(); }
    }

    public string MaximumPrice
    {
        get => _maximumPrice;
        set { if (SetProperty(ref _maximumPrice, value)) ApplyFilters(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string FilterMessage
    {
        get => _filterMessage;
        private set => SetProperty(ref _filterMessage, value);
    }

    public IReadOnlyList<ScienceParkCardViewModel> Results
    {
        get => _results;
        private set
        {
            if (SetProperty(ref _results, value))
            {
                OnPropertyChanged(nameof(ResultCountText));
                OnPropertyChanged(nameof(HasNoResults));
            }
        }
    }

    public string ResultCountText => $"{Results.Count} parque(s) encontrado(s)";
    public string SelectedCountText => $"{_selection.Count} parque(s) selecionado(s)";
    public bool HasNoResults => Results.Count == 0;

    public async Task LoadAsync()
    {
        StatusMessage = "A carregar parques...";
        try
        {
            var data = await _catalog.LoadAsync();
            _allParks = data.Parks;
            StatusMessage = data.IsDemonstration
                ? "DEMONSTRAÇÃO · Os parques, serviços e preços desta página são fictícios."
                : "Dados do catálogo de parques.";
            ApplyFilters();
        }
        catch (Exception)
        {
            _allParks = [];
            Results = [];
            StatusMessage = "Não foi possível carregar os parques. Tente atualizar.";
        }
    }

    private void ApplyFilters()
    {
        if (!TryReadPrice(MinimumPrice, out var minimum) || !TryReadPrice(MaximumPrice, out var maximum))
        {
            FilterMessage = "Introduza preços válidos, iguais ou superiores a zero.";
            Results = [];
            return;
        }

        if (minimum > maximum)
        {
            FilterMessage = "O preço mínimo não pode ser superior ao máximo.";
            Results = [];
            return;
        }

        FilterMessage = string.Empty;
        var required = ServiceFilters.Where(filter => filter.IsSelected).Select(filter => filter.Kind).ToArray();
        Results = ScienceParkSearch.Filter(_allParks, NameQuery, LocationQuery, required, minimum, maximum)
            .Select(park => new ScienceParkCardViewModel(park, _selection))
            .ToArray();
    }

    private static bool TryReadPrice(string text, out decimal? price)
    {
        price = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (!decimal.TryParse(text, NumberStyles.Number, PortugueseCulture, out var value) || value < 0) return false;
        price = value;
        return true;
    }

    private void ClearFilters()
    {
        NameQuery = string.Empty;
        LocationQuery = string.Empty;
        MinimumPrice = string.Empty;
        MaximumPrice = string.Empty;
        foreach (var service in ServiceFilters) service.IsSelected = false;
        ApplyFilters();
    }

    private void ShowDetails(ScienceParkCardViewModel? park)
    {
        if (park is null) return;
        _pageState.DetailParkId = park.Id;
        _navigation.NavigateTo(PageKeys.ScienceParkDetails);
    }
}
