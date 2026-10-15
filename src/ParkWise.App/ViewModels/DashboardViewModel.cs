using System.Globalization;
using System.Windows.Input;
using ParkWise.Application.Dashboard;

namespace ParkWise.App.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-PT");
    private readonly IDashboardService _dashboardService;
    private DashboardSummary? _summary;
    private string _statusMessage = "A carregar indicadores...";

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ICommand RefreshCommand { get; }

    public DashboardSummary? Summary
    {
        get => _summary;
        private set
        {
            if (!SetProperty(ref _summary, value)) return;
            OnPropertyChanged(nameof(AveragePriceText));
            OnPropertyChanged(nameof(HasPriceDistribution));
            OnPropertyChanged(nameof(HasNoPriceDistribution));
            OnPropertyChanged(nameof(PricedParkCount));
            OnPropertyChanged(nameof(Indicators));
            OnPropertyChanged(nameof(RecentParks));
            OnPropertyChanged(nameof(RecentSimulations));
            OnPropertyChanged(nameof(PriceDistribution));
            OnPropertyChanged(nameof(HasNoRecentParks));
            OnPropertyChanged(nameof(HasNoRecentSimulations));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string AveragePriceText => Summary?.AveragePricePerSquareMetre is decimal value
        ? $"{value.ToString("N2", PortugueseCulture)} € / m²"
        : "—";

    public bool HasPriceDistribution => Summary?.HasPriceDistribution == true;
    public bool HasNoPriceDistribution => Summary is not null && !HasPriceDistribution;
    public int PricedParkCount => Summary?.PriceDistribution.Sum(band => band.Count) ?? 0;
    public IReadOnlyList<PriceBand> PriceDistribution => Summary?.PriceDistribution ?? [];
    public bool HasNoRecentParks => Summary is not null && RecentParks.Count == 0;
    public bool HasNoRecentSimulations => Summary is not null && RecentSimulations.Count == 0;

    public IReadOnlyList<DashboardIndicator> Indicators =>
    [
        new("Parques registados", Summary?.TotalParks.ToString(PortugueseCulture) ?? "—", "Total no catálogo", "\uE80D"),
        new("Simulações realizadas", Summary?.TotalSimulations.ToString(PortugueseCulture) ?? "—", "Simulações guardadas", "\uE7C3"),
        new("Disponíveis para comparação", Summary?.ComparableParks.ToString(PortugueseCulture) ?? "—", "Preço válido e 9 critérios avaliados", "\uE8B7"),
        new("Preço médio de aluguer", AveragePriceText, "Por m² e por mês", "\uE8D4")
    ];

    public IReadOnlyList<DashboardParkRow> RecentParks => Summary?.RecentParks
        .Select(park => new DashboardParkRow(
            park.Name,
            park.City,
            park.MonthlyRentPerSquareMetre is decimal price
                ? $"{price.ToString("N2", PortugueseCulture)} € / m²"
                : "Sem preço",
            ToLocalDate(park.CreatedAtUtc)))
        .ToArray() ?? [];

    public IReadOnlyList<DashboardSimulationRow> RecentSimulations => Summary?.RecentSimulations
        .Select(simulation => new DashboardSimulationRow(
            simulation.Name,
            ToLocalDate(simulation.CreatedAtUtc)))
        .ToArray() ?? [];

    private static string ToLocalDate(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy", PortugueseCulture);

    public async Task LoadAsync()
    {
        StatusMessage = "A carregar indicadores...";
        try
        {
            Summary = await _dashboardService.GetSummaryAsync();
            StatusMessage = Summary.IsDemonstration
                ? "Demonstração: todos os parques, simulações e preços apresentados são fictícios."
                : "Dados provenientes da base de dados configurada.";
        }
        catch (Exception)
        {
            Summary = null;
            StatusMessage = "Não foi possível carregar os indicadores. Verifique a ligação à base de dados e tente atualizar.";
        }
    }
}

public sealed record DashboardIndicator(string Label, string Value, string Caption, string Icon);
public sealed record DashboardParkRow(string Name, string City, string Price, string CreatedAt);
public sealed record DashboardSimulationRow(string Name, string CreatedAt);
