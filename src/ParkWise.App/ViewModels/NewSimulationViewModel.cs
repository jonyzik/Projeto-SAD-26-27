using System.Collections.ObjectModel;
using System.Windows.Input;
using ParkWise.App.Navigation;
using ParkWise.Application.Simulations;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.App.ViewModels;

/// <summary>
/// Página «Nova simulação»: necessidades da empresa, parâmetros de custo, seleção de parques,
/// comparações AHP entre critérios, execução e apresentação dos resultados.
/// </summary>
public sealed class NewSimulationViewModel : ViewModelBase
{
    private readonly ISimulationService _service;
    private readonly SimulationSession _session;
    private readonly Action<string> _navigate;
    private bool _suspendRecalculation;

    private string _name = string.Empty;
    private string? _description;
    private string _areaText = string.Empty;
    private string _monthsText = string.Empty;

    private string _statusMessage = "A carregar parques...";
    private string _weightsSummary = string.Empty;
    private string _selectionSummary = string.Empty;
    private SimulationOutcome? _outcome;
    private ResultRowViewModel? _selectedResult;
    private string _resultsNotice = string.Empty;
    private bool _isDemonstration;
    private bool _isLoaded;

    public NewSimulationViewModel(ISimulationService service, SimulationSession session, Action<string> navigate)
    {
        _service = service;
        _session = session;
        _navigate = navigate;

        foreach (var (kind, name) in EvaluationCriteria.Required)
            Criteria.Add(new CriterionPreferenceViewModel(kind, name, RecalculateWeights));

        InitializeCommand = new AsyncRelayCommand(InitializeAsync, exception =>
        {
            System.Diagnostics.Trace.TraceError($"New simulation initialization failed: {exception}");
            StatusMessage = "Não foi possível carregar a página. Verifique a ligação.";
        });
        RunCommand = new AsyncParameterCommand(_ => RunAsync(), _ => _isLoaded);
        SaveCommand = new AsyncParameterCommand(_ => SaveAsync(), _ => CanSave);
        SelectAllParksCommand = new RelayCommand(_ => SetAllParks(true));
        ClearParksCommand = new RelayCommand(_ => SetAllParks(false));
        ResetImportanceCommand = new RelayCommand(_ => ResetImportance());
        BackToListCommand = new RelayCommand(_ => _navigate(PageKeys.Simulations));

        RecalculateWeights();
    }

    public ObservableCollection<SelectableParkViewModel> Parks { get; } = [];
    public ObservableCollection<CriterionPreferenceViewModel> Criteria { get; } = [];
    public ObservableCollection<ResultRowViewModel> Results { get; } = [];
    public ObservableCollection<string> Errors { get; } = [];

    public ICommand InitializeCommand { get; }
    public AsyncParameterCommand RunCommand { get; }
    public AsyncParameterCommand SaveCommand { get; }
    public ICommand SelectAllParksCommand { get; }
    public ICommand ClearParksCommand { get; }
    public ICommand ResetImportanceCommand { get; }
    public ICommand BackToListCommand { get; }

    #region Identificação, necessidades e parâmetros

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string? Description { get => _description; set => SetProperty(ref _description, value); }
    public string AreaText { get => _areaText; set => SetInput(ref _areaText, value); }
    public string MonthsText { get => _monthsText; set => SetInput(ref _monthsText, value); }

    #endregion

    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsDemonstration { get => _isDemonstration; private set => SetProperty(ref _isDemonstration, value); }
    public string WeightsSummary { get => _weightsSummary; private set => SetProperty(ref _weightsSummary, value); }
    public string SelectionSummary { get => _selectionSummary; private set => SetProperty(ref _selectionSummary, value); }
    public string ResultsNotice { get => _resultsNotice; private set => SetProperty(ref _resultsNotice, value); }
    public bool HasErrors => Errors.Count > 0;
    public bool HasResults => Results.Count > 0;
    public bool CanSave => _outcome is { Id: null } && Results.Count > 0;

    public string BestParkText => _outcome?.BestResult is { } best
        ? $"{best.ParkName} — {NumberInput.Percent(best.Score)} da prioridade global, {NumberInput.Money(best.Costs?.Monthly)} / mês"
        : "Nenhum parque cumpre as necessidades indicadas.";


    public ResultRowViewModel? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (!SetProperty(ref _selectedResult, value)) return;
            OnPropertyChanged(nameof(HasSelectedResult));
        }
    }

    public bool HasSelectedResult => SelectedResult is not null;

    public async Task InitializeAsync()
    {
        try
        {
            var catalog = await _service.GetCandidateParksAsync();
            IsDemonstration = catalog.IsDemonstration;
            Parks.Clear();
            foreach (var park in catalog.Parks) Parks.Add(new SelectableParkViewModel(park, OnParkSelectionChanged));

            var (simulationId, asCopy) = _session.Consume();
            SimulationOutcome? saved = simulationId is Guid id ? await _service.GetAsync(id) : null;
            if (saved is not null)
            {
                LoadFromOutcome(saved, asCopy);
            }
            else
            {
                ApplyNeeds(CompanyNeeds.Default);
                Name = $"Simulação de {DateTime.Now.ToString("dd/MM/yyyy HH:mm", NumberInput.Portuguese)}";
                StatusMessage = simulationId is null
                    ? DefaultStatus()
                    : "A simulação pedida já não existe. Foi aberta uma nova simulação.";
            }
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível carregar os parques. Verifique a ligação à base de dados.";
        }
        finally
        {
            _isLoaded = true;
            RunCommand.RaiseCanExecuteChanged();
            OnParkSelectionChanged();
        }
    }

    private string DefaultStatus() => IsDemonstration
        ? "Demonstração: os parques apresentados são fictícios."
        : "Defina as necessidades da empresa, selecione os parques e indique a importância de cada critério.";

    private void LoadFromOutcome(SimulationOutcome saved, bool asCopy)
    {
        _suspendRecalculation = true;
        try
        {
            ApplyNeeds(saved.Needs);
            Name = asCopy ? $"Cópia de {saved.Name}" : saved.Name;
            Description = saved.Description;

            var selected = saved.CandidateParkIds.ToHashSet();
            foreach (var park in Parks) park.IsSelected = selected.Contains(park.Id);

            var mandatory = saved.MandatoryCriteria.ToHashSet();
            foreach (var criterion in Criteria) criterion.IsMandatory = mandatory.Contains(criterion.Kind);

            var importance = saved.CriterionImportance;
            foreach (var criterion in Criteria)
                criterion.Importance = importance.TryGetValue(criterion.Kind, out var value) ? value : ImportanceScale.Default;
        }
        finally
        {
            _suspendRecalculation = false;
        }
        RecalculateWeights();

        var missing = saved.CandidateParkIds.Count(id => Parks.All(p => p.Id != id));
        if (asCopy)
        {
            ClearResults();
            StatusMessage = "Cópia criada a partir de uma simulação guardada. Ajuste os parâmetros e execute.";
        }
        else
        {
            ShowOutcome(saved);
            var hasDetail = saved.Results.Where(r => r.IsEligible).All(r => r.Contributions.Count > 0);
            ResultsNotice = $"Resultados guardados em {NumberInput.LocalDate(saved.ExecutedAtUtc)}." + (hasDetail
                ? " Se executar novamente, será criada uma nova simulação."
                : " Execute novamente para ver a contribuição de cada critério (será criada uma nova simulação).");
            StatusMessage = $"Simulação «{saved.Name}» aberta.";
        }
        if (missing > 0)
            StatusMessage += $" {missing} parque(s) desta simulação já não existe(m) no catálogo.";
    }

    private void ApplyNeeds(CompanyNeeds needs)
    {
        _suspendRecalculation = true;
        try
        {
            AreaText = NumberInput.Format(needs.RequiredAreaSquareMetres);
            MonthsText = needs.ContractDurationMonths.ToString(NumberInput.Portuguese);
        }
        finally
        {
            _suspendRecalculation = false;
        }
        UpdateCostEstimates();
    }

    /// <summary>Converte a área e a duração do contrato do formulário; devolve os erros de formato encontrados.</summary>
    private CompanyNeeds? TryReadNeeds(List<string> errors)
    {
        var initialErrors = errors.Count;
        if (!NumberInput.TryParseDecimal(AreaText, out var area))
            errors.Add("«Área necessária» tem de ser um número.");
        if (!NumberInput.TryParseInt(MonthsText, out var months))
            errors.Add("«Duração do contrato» tem de ser um número inteiro.");
        return errors.Count > initialErrors ? null : new CompanyNeeds(area, months);
    }

    private SimulationRequest? TryBuildRequest(List<string> errors)
    {
        var needs = TryReadNeeds(errors);
        if (needs is null) return null;
        return new SimulationRequest(
            Name, Description, needs,
            Parks.Where(p => p.IsSelected).Select(p => p.Id).ToArray(),
            CurrentImportance(),
            Criteria.Where(c => c.IsMandatory).Select(c => c.Kind).ToArray());
    }

    private async Task RunAsync()
    {
        var errors = new List<string>();
        var request = TryBuildRequest(errors);
        if (request is not null) errors.AddRange(SimulationRequestValidator.Validate(request));
        SetErrors(errors);
        if (errors.Count > 0 || request is null)
        {
            StatusMessage = "Corrija os dados assinalados antes de executar a simulação.";
            return;
        }

        StatusMessage = "A executar a simulação...";
        try
        {
            var outcome = await _service.RunAsync(request);
            ShowOutcome(outcome);
            ResultsNotice = outcome.IsDemonstration
                ? "Resultados calculados com parques fictícios."
                : string.Empty;
            StatusMessage = outcome.RankedResults.Count > 0
                ? "Simulação executada. Reveja os resultados e guarde-a, se pretender."
                : "Simulação executada, mas nenhum parque cumpre as necessidades indicadas.";
        }
        catch (SimulationValidationException exception)
        {
            SetErrors(exception.Errors);
            StatusMessage = "Corrija os dados assinalados antes de executar a simulação.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível executar a simulação. Verifique a ligação à base de dados.";
        }
    }

    private async Task SaveAsync()
    {
        if (_outcome is null || _outcome.Id is not null) return;
        try
        {
            var id = await _service.SaveAsync(_outcome);
            _outcome = _outcome with { Id = id };
            StatusMessage = $"Simulação «{_outcome.Name}» guardada.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível guardar a simulação. Verifique a ligação à base de dados.";
        }
        RaiseOutcomeChanged();
    }

    private void ShowOutcome(SimulationOutcome outcome)
    {
        _outcome = outcome;
        Results.Clear();
        var maxScore = outcome.RankedResults.Count > 0 ? outcome.RankedResults.Max(r => r.Score) : 0;
        foreach (var result in outcome.Results) Results.Add(new ResultRowViewModel(result, maxScore));
        SelectedResult = Results.FirstOrDefault();
        RaiseOutcomeChanged();
    }

    private void ClearResults()
    {
        _outcome = null;
        Results.Clear();
        SelectedResult = null;
        ResultsNotice = string.Empty;
        RaiseOutcomeChanged();
    }

    private void RaiseOutcomeChanged()
    {
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(BestParkText));
        SaveCommand.RaiseCanExecuteChanged();
    }

    private void SetErrors(IEnumerable<string> errors)
    {
        Errors.Clear();
        foreach (var error in errors.Distinct()) Errors.Add(error);
        OnPropertyChanged(nameof(HasErrors));
    }

    private void SetInput(ref string field, string value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        if (!SetProperty(ref field, value, name)) return;
        if (!_suspendRecalculation) UpdateCostEstimates();
    }

    private void UpdateCostEstimates()
    {
        var needs = TryReadNeeds([]);
        var valid = needs is not null && SimulationRequestValidator.ValidateNeeds(needs).Count == 0;
        foreach (var park in Parks)
        {
            park.EstimatedMonthlyCost = valid && park.Rent is > 0
                ? $"{NumberInput.Money(_service.EstimateCosts(park.Rent.Value, needs!).Monthly)} / mês"
                : "—";
        }
    }

    private void OnParkSelectionChanged()
    {
        var selected = Parks.Count(p => p.IsSelected);
        SelectionSummary = Parks.Count == 0
            ? "Não existem parques registados."
            : $"{selected} de {Parks.Count} parques selecionados (mínimo {SimulationRequestValidator.MinimumCandidateParks}).";
    }

    private void SetAllParks(bool selected)
    {
        foreach (var park in Parks) park.IsSelected = selected;
    }

    private IReadOnlyDictionary<CriterionKind, int> CurrentImportance() =>
        Criteria.ToDictionary(c => c.Kind, c => c.ImportanceValue);

    private void ResetImportance()
    {
        _suspendRecalculation = true;
        try
        {
            foreach (var criterion in Criteria) criterion.Importance = ImportanceScale.Default;
        }
        finally
        {
            _suspendRecalculation = false;
        }
        RecalculateWeights();
    }

    private void RecalculateWeights()
    {
        if (_suspendRecalculation) return;
        var preview = _service.PreviewWeights(
            CurrentImportance(),
            Criteria.Where(c => c.IsMandatory).Select(c => c.Kind).ToArray());

        var max = preview.Weights.Max(w => w.Weight);
        foreach (var criterion in Criteria)
        {
            var weight = preview.Weights.Single(w => w.Kind == criterion.Kind).Weight;
            criterion.Weight = weight;
            criterion.BarValue = max > 0 ? (double)(weight / max) : 0;
        }

        var top = Criteria.Where(c => c.ImportanceValue == Criteria.Max(x => x.ImportanceValue)).Select(c => c.Name).ToArray();
        WeightsSummary = Criteria.All(c => c.ImportanceValue == Criteria[0].ImportanceValue)
            ? "Todos os critérios têm a mesma importância: cada um pesa 1/9 na decisão."
            : $"Critério{(top.Length > 1 ? "s" : "")} com mais peso: {string.Join(", ", top)}.";
    }
}
