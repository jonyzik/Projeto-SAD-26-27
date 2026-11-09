using System.Collections.ObjectModel;
using System.Windows.Input;
using ParkWise.App.Navigation;
using ParkWise.Application.Simulations;

namespace ParkWise.App.ViewModels;

/// <summary>Página «Simulações»: lista as simulações guardadas e permite abrir, duplicar, eliminar ou criar.</summary>
public sealed class SimulationsViewModel : ViewModelBase
{
    private readonly ISimulationService _service;
    private readonly SimulationSession _session;
    private readonly Action<string> _navigate;
    private readonly Func<string, bool> _confirm;
    private string _statusMessage = "A carregar simulações...";
    private bool _isLoaded;
    private bool _isDemonstration;

    public SimulationsViewModel(ISimulationService service, SimulationSession session,
        Action<string> navigate, Func<string, bool> confirm)
    {
        _service = service;
        _session = session;
        _navigate = navigate;
        _confirm = confirm;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        NewCommand = new RelayCommand(_ =>
        {
            _session.Clear();
            _navigate(PageKeys.NewSimulation);
        });
        OpenCommand = new RelayCommand(parameter => Open(parameter, asCopy: false));
        DuplicateCommand = new RelayCommand(parameter => Open(parameter, asCopy: true));
        DeleteCommand = new AsyncParameterCommand(DeleteAsync);
    }

    public ObservableCollection<SimulationRowViewModel> Simulations { get; } = [];

    public ICommand LoadCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand DuplicateCommand { get; }
    public ICommand DeleteCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsDemonstration
    {
        get => _isDemonstration;
        private set => SetProperty(ref _isDemonstration, value);
    }

    public bool HasNoSimulations => _isLoaded && Simulations.Count == 0;

    public async Task LoadAsync()
    {
        StatusMessage = "A carregar simulações...";
        try
        {
            var catalog = await _service.GetCandidateParksAsync();
            var items = await _service.ListAsync();
            IsDemonstration = catalog.IsDemonstration;
            Simulations.Clear();
            foreach (var item in items) Simulations.Add(new SimulationRowViewModel(item));
            StatusMessage = IsDemonstration
                ? "Demonstração: parques fictícios. As simulações guardadas ficam em memória até fechar a aplicação."
                : $"{Simulations.Count} simulação(ões) guardada(s) na base de dados.";
        }
        catch (Exception)
        {
            Simulations.Clear();
            StatusMessage = "Não foi possível carregar as simulações. Verifique a ligação à base de dados e tente atualizar.";
        }
        finally
        {
            _isLoaded = true;
            OnPropertyChanged(nameof(HasNoSimulations));
        }
    }

    private void Open(object? parameter, bool asCopy)
    {
        if (parameter is not SimulationRowViewModel row) return;
        _session.RequestOpen(row.Id, asCopy);
        _navigate(PageKeys.NewSimulation);
    }

    private async Task DeleteAsync(object? parameter)
    {
        if (parameter is not SimulationRowViewModel row) return;
        if (!_confirm($"Eliminar a simulação «{row.Name}»? Esta ação não pode ser anulada.")) return;
        try
        {
            if (await _service.DeleteAsync(row.Id)) Simulations.Remove(row);
            StatusMessage = $"Simulação «{row.Name}» eliminada.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível eliminar a simulação.";
        }
        OnPropertyChanged(nameof(HasNoSimulations));
    }
}

public sealed class SimulationRowViewModel(SimulationListItem item)
{
    public Guid Id { get; } = item.Id;
    public string Name { get; } = item.Name;
    public string CreatedAt { get; } = NumberInput.LocalDate(item.CreatedAtUtc);
    public string Candidates { get; } = $"{item.EligibleCount} de {item.CandidateCount} elegíveis";
    public string BestPark { get; } = item.BestParkName ?? "Nenhum parque elegível";
    public string BestParkCost { get; } = item.BestParkMonthlyCost is decimal cost ? $"{NumberInput.Money(cost)} / mês" : "—";
    public string Consistency { get; } = item.ConsistencyRatio is decimal ratio
        ? $"RC {ratio.ToString("0.000", NumberInput.Portuguese)}"
        : "—";
}
