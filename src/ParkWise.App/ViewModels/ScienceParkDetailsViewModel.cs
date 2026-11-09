using System.Windows.Input;
using ParkWise.App.Navigation;
using ParkWise.Application.ScienceParks;

namespace ParkWise.App.ViewModels;

public sealed class ScienceParkDetailsViewModel : ViewModelBase
{
    private readonly IScienceParkCatalog _catalog;
    private readonly ScienceParkSelection _selection;
    private readonly ScienceParkPageState _pageState;
    private readonly IPageNavigation _navigation;
    private SciencePark? _park;
    private string _statusMessage = string.Empty;

    public ScienceParkDetailsViewModel(
        IScienceParkCatalog catalog,
        ScienceParkSelection selection,
        ScienceParkPageState pageState,
        IPageNavigation navigation)
    {
        _catalog = catalog;
        _selection = selection;
        _pageState = pageState;
        _navigation = navigation;
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo(PageKeys.ScienceParks));
        OpenComparisonCommand = new RelayCommand(_ => _navigation.NavigateTo(PageKeys.ScienceParkComparison));
    }

    public ICommand BackCommand { get; }
    public ICommand OpenComparisonCommand { get; }

    public SciencePark? Park
    {
        get => _park;
        private set
        {
            if (!SetProperty(ref _park, value)) return;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(City));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(PriceText));
            OnPropertyChanged(nameof(Criteria));
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(HasPark));
        }
    }

    public bool HasPark => Park is not null;
    public string Name => Park?.Name ?? string.Empty;
    public string City => Park?.City ?? string.Empty;
    public string Description => Park?.Description ?? string.Empty;
    public string PriceText => ScienceParkPresentation.Price(Park?.MonthlyRentPerSquareMetre);
    public IReadOnlyList<ScienceParkCriterionRow> Criteria => Park is null ? [] : ScienceParkPresentation.Criteria(Park);

    public bool IsSelected
    {
        get => Park is not null && _selection.Contains(Park.Id);
        set
        {
            if (Park is null) return;
            _selection.SetSelected(Park.Id, value);
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync()
    {
        Park = null;
        if (_pageState.DetailParkId is not Guid id)
        {
            StatusMessage = "Selecione um parque na pesquisa para consultar os detalhes.";
            return;
        }

        try
        {
            var data = await _catalog.LoadAsync();
            Park = data.Parks.FirstOrDefault(park => park.Id == id);
            StatusMessage = Park is null
                ? "O parque selecionado já não está disponível no catálogo."
                : data.IsDemonstration
                    ? "DEMONSTRAÇÃO · Dados fictícios. A disponibilidade dos serviços é apenas ilustrativa."
                    : "Dados do catálogo de parques.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível consultar este parque. Tente novamente.";
        }
    }
}
