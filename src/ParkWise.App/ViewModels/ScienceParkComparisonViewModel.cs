using System.Windows.Input;
using ParkWise.App.Navigation;
using ParkWise.Application.ScienceParks;

namespace ParkWise.App.ViewModels;

public sealed class ScienceParkComparisonViewModel : ViewModelBase
{
    private readonly IScienceParkCatalog _catalog;
    private readonly ScienceParkSelection _selection;
    private readonly IPageNavigation _navigation;
    private IReadOnlyList<ScienceParkCardViewModel> _parks = [];
    private string _statusMessage = string.Empty;

    public ScienceParkComparisonViewModel(
        IScienceParkCatalog catalog,
        ScienceParkSelection selection,
        IPageNavigation navigation)
    {
        _catalog = catalog;
        _selection = selection;
        _navigation = navigation;
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo(PageKeys.ScienceParks));
        StartSimulationCommand = new RelayCommand(_ =>
        {
            if (CanPrepareSimulation) _navigation.NavigateTo(PageKeys.NewSimulation);
        });
    }

    public ICommand BackCommand { get; }
    public ICommand StartSimulationCommand { get; }

    public IReadOnlyList<ScienceParkCardViewModel> Parks
    {
        get => _parks;
        private set
        {
            if (!SetProperty(ref _parks, value)) return;
            OnPropertyChanged(nameof(CountText));
            OnPropertyChanged(nameof(CanPrepareSimulation));
            OnPropertyChanged(nameof(HasNoParks));
        }
    }

    public string CountText => $"{Parks.Count} parque(s) selecionado(s)";
    public bool HasNoParks => Parks.Count == 0;
    public bool CanPrepareSimulation => Parks.Count >= 2;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            var data = await _catalog.LoadAsync();
            var byId = data.Parks.ToDictionary(park => park.Id);
            Parks = _selection.SelectedIds
                .Where(byId.ContainsKey)
                .Select(id => new ScienceParkCardViewModel(byId[id], _selection))
                .ToArray();
            StatusMessage = data.IsDemonstration
                ? "DEMONSTRAÇÃO · Comparação descritiva de parques e preços fictícios. Sem pontuação ou AHP."
                : "Comparação descritiva, sem pontuação ou AHP.";
        }
        catch (Exception)
        {
            Parks = [];
            StatusMessage = "Não foi possível carregar os parques selecionados. Tente voltar à pesquisa.";
        }
    }
}
