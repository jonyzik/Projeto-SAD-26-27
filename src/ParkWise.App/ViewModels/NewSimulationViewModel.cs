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
    private string _employeesText = string.Empty;
    private string _monthsText = string.Empty;
    private string _budgetText = string.Empty;
    private string _preferredCity = string.Empty;
    private string _serviceChargeText = string.Empty;
    private string _utilitiesText = string.Empty;
    private string _servicesPerEmployeeText = string.Empty;
    private string _parkingPercentText = string.Empty;
    private string _parkingCostText = string.Empty;
    private string _setupCostText = string.Empty;
    private string _contingencyText = string.Empty;

    private string _statusMessage = "A carregar parques e parâmetros...";
    private string _consistencyText = string.Empty;
    private bool _isConsistent = true;
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

        var names = EvaluationCriteria.Required.ToDictionary(c => c.Kind, c => c.Name);
        foreach (var (left, right) in PairwiseComparisonMatrix.Pairs(EvaluationCriteria.Required.Select(c => c.Kind).ToArray()))
            Comparisons.Add(new PairwiseComparisonViewModel(left, names[left], right, names[right], RecalculateWeights));

        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        RunCommand = new AsyncParameterCommand(_ => RunAsync(), _ => _isLoaded);
        SaveCommand = new AsyncParameterCommand(_ => SaveAsync(), _ => CanSave);
        LoadDefaultsCommand = new AsyncParameterCommand(_ => LoadDefaultsAsync());
        SaveDefaultsCommand = new AsyncParameterCommand(_ => SaveDefaultsAsync());
        FactoryDefaultsCommand = new AsyncParameterCommand(_ => FactoryDefaultsAsync());
        SelectAllParksCommand = new RelayCommand(_ => SetAllParks(true));
        ClearParksCommand = new RelayCommand(_ => SetAllParks(false));
        ResetComparisonsCommand = new RelayCommand(_ => ResetComparisons());
        BackToListCommand = new RelayCommand(_ => _navigate(PageKeys.Simulations));

        RecalculateWeights();
    }

    public ObservableCollection<SelectableParkViewModel> Parks { get; } = [];
    public ObservableCollection<CriterionPreferenceViewModel> Criteria { get; } = [];
    public ObservableCollection<PairwiseComparisonViewModel> Comparisons { get; } = [];
    public ObservableCollection<ResultRowViewModel> Results { get; } = [];
    public ObservableCollection<string> Errors { get; } = [];

    public ICommand InitializeCommand { get; }
    public AsyncParameterCommand RunCommand { get; }
    public AsyncParameterCommand SaveCommand { get; }
    public ICommand LoadDefaultsCommand { get; }
    public ICommand SaveDefaultsCommand { get; }
    public ICommand FactoryDefaultsCommand { get; }
    public ICommand SelectAllParksCommand { get; }
    public ICommand ClearParksCommand { get; }
    public ICommand ResetComparisonsCommand { get; }
    public ICommand BackToListCommand { get; }

    #region Identificação, necessidades e parâmetros

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string? Description { get => _description; set => SetProperty(ref _description, value); }
    public string AreaText { get => _areaText; set => SetInput(ref _areaText, value); }
    public string EmployeesText { get => _employeesText; set => SetInput(ref _employeesText, value); }
    public string MonthsText { get => _monthsText; set => SetInput(ref _monthsText, value); }
    public string BudgetText { get => _budgetText; set => SetInput(ref _budgetText, value); }
    public string PreferredCity { get => _preferredCity; set => SetProperty(ref _preferredCity, value); }
    public string ServiceChargeText { get => _serviceChargeText; set => SetInput(ref _serviceChargeText, value); }
    public string UtilitiesText { get => _utilitiesText; set => SetInput(ref _utilitiesText, value); }
    public string ServicesPerEmployeeText { get => _servicesPerEmployeeText; set => SetInput(ref _servicesPerEmployeeText, value); }
    public string ParkingPercentText { get => _parkingPercentText; set => SetInput(ref _parkingPercentText, value); }
    public string ParkingCostText { get => _parkingCostText; set => SetInput(ref _parkingCostText, value); }
    public string SetupCostText { get => _setupCostText; set => SetInput(ref _setupCostText, value); }
    public string ContingencyText { get => _contingencyText; set => SetInput(ref _contingencyText, value); }

    #endregion

    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsDemonstration { get => _isDemonstration; private set => SetProperty(ref _isDemonstration, value); }
    public string ConsistencyText { get => _consistencyText; private set => SetProperty(ref _consistencyText, value); }
    public bool IsConsistent { get => _isConsistent; private set => SetProperty(ref _isConsistent, value); }
    public string SelectionSummary { get => _selectionSummary; private set => SetProperty(ref _selectionSummary, value); }
    public string ResultsNotice { get => _resultsNotice; private set => SetProperty(ref _resultsNotice, value); }
    public bool HasErrors => Errors.Count > 0;
    public bool HasResults => Results.Count > 0;
    public bool CanSave => _outcome is { Id: null } && Results.Count > 0;

    public string BestParkText => _outcome?.BestResult is { } best
        ? $"{best.ParkName} — {NumberInput.Percent(best.Score)} da prioridade global, {NumberInput.Money(best.MonthlyCost)} / mês"
        : "Nenhum parque cumpre as necessidades indicadas.";

    public string OutcomeConsistencyText => _outcome is null
        ? string.Empty
        : $"Razão de consistência: {_outcome.Consistency.ConsistencyRatio.ToString("0.000", NumberInput.Portuguese)}";

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
                ApplyParameters(await _service.GetDefaultParametersAsync());
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
            ApplyParameters(saved.Parameters);
            Name = asCopy ? $"Cópia de {saved.Name}" : saved.Name;
            Description = saved.Description;

            var selected = saved.CandidateParkIds.ToHashSet();
            foreach (var park in Parks) park.IsSelected = selected.Contains(park.Id);

            var mandatory = saved.MandatoryCriteria.ToHashSet();
            foreach (var criterion in Criteria) criterion.IsMandatory = mandatory.Contains(criterion.Kind);

            foreach (var comparison in Comparisons) comparison.Position = 0;
            foreach (var stored in saved.Comparisons)
            {
                var match = Comparisons.FirstOrDefault(c => c.Left == stored.Left && c.Right == stored.Right);
                if (match is not null) match.Load(stored.Value);
                else Comparisons.FirstOrDefault(c => c.Left == stored.Right && c.Right == stored.Left)?.Load(1m / stored.Value);
            }
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
            var hasDetail = saved.Results.Where(r => r.IsEligible).All(r => r.CostBreakdown is not null);
            ResultsNotice = $"Resultados guardados em {NumberInput.LocalDate(saved.ExecutedAtUtc)}." + (hasDetail
                ? " Se executar novamente, será criada uma nova simulação."
                : " Execute novamente para ver o detalhe dos custos e das contribuições (será criada uma nova simulação).");
            StatusMessage = $"Simulação «{saved.Name}» aberta.";
        }
        if (missing > 0)
            StatusMessage += $" {missing} parque(s) desta simulação já não existe(m) no catálogo.";
    }

    private void ApplyParameters(SimulationParameters parameters)
    {
        var needs = parameters.Needs;
        var costs = parameters.Costs;
        _suspendRecalculation = true;
        try
        {
            AreaText = NumberInput.Format(needs.RequiredAreaSquareMetres);
            EmployeesText = needs.EmployeeCount.ToString(NumberInput.Portuguese);
            MonthsText = needs.ContractDurationMonths.ToString(NumberInput.Portuguese);
            BudgetText = needs.MaxMonthlyBudget is decimal budget ? NumberInput.Format(budget) : string.Empty;
            PreferredCity = needs.PreferredCity ?? string.Empty;
            ServiceChargeText = NumberInput.Format(costs.ServiceChargePerSquareMetre);
            UtilitiesText = NumberInput.Format(costs.UtilitiesPerSquareMetre);
            ServicesPerEmployeeText = NumberInput.Format(costs.ServicesPerEmployee);
            ParkingPercentText = NumberInput.Format(costs.ParkingSpacesRatio * 100m);
            ParkingCostText = NumberInput.Format(costs.ParkingCostPerSpace);
            SetupCostText = NumberInput.Format(costs.SetupCostPerSquareMetre);
            ContingencyText = NumberInput.Format(costs.ContingencyPercentage);
        }
        finally
        {
            _suspendRecalculation = false;
        }
        UpdateCostEstimates();
    }

    /// <summary>Converte os campos do formulário em parâmetros; devolve os erros de formato encontrados.</summary>
    private SimulationParameters? TryReadParameters(List<string> errors)
    {
        decimal Decimal(string text, string label, decimal fallback = 0)
        {
            if (NumberInput.TryParseDecimal(text, out var value)) return value;
            errors.Add($"«{label}» tem de ser um número.");
            return fallback;
        }

        int Integer(string text, string label)
        {
            if (NumberInput.TryParseInt(text, out var value)) return value;
            errors.Add($"«{label}» tem de ser um número inteiro.");
            return 0;
        }

        var initialErrors = errors.Count;
        var area = Decimal(AreaText, "Área necessária");
        var employees = Integer(EmployeesText, "Número de colaboradores");
        var months = Integer(MonthsText, "Duração do contrato");
        decimal? budget = string.IsNullOrWhiteSpace(BudgetText) ? null : Decimal(BudgetText, "Orçamento mensal máximo");
        var serviceCharge = Decimal(ServiceChargeText, "Condomínio e serviços comuns");
        var utilities = Decimal(UtilitiesText, "Energia e água");
        var servicesPerEmployee = Decimal(ServicesPerEmployeeText, "Serviços por colaborador");
        var parkingPercent = Decimal(ParkingPercentText, "Colaboradores com estacionamento");
        var parkingCost = Decimal(ParkingCostText, "Custo por lugar de estacionamento");
        var setupCost = Decimal(SetupCostText, "Instalação");
        var contingency = Decimal(ContingencyText, "Contingência");
        if (errors.Count > initialErrors) return null;

        return new SimulationParameters(
            new CompanyNeeds(area, employees, months, budget,
                string.IsNullOrWhiteSpace(PreferredCity) ? null : PreferredCity.Trim()),
            new CostParameters(serviceCharge, utilities, servicesPerEmployee, parkingPercent / 100m,
                parkingCost, setupCost, contingency));
    }

    private SimulationRequest? TryBuildRequest(List<string> errors)
    {
        var parameters = TryReadParameters(errors);
        if (parameters is null) return null;
        return new SimulationRequest(
            Name, Description, parameters,
            Parks.Where(p => p.IsSelected).Select(p => p.Id).ToArray(),
            Comparisons.Where(c => !c.IsNeutral).Select(c => c.ToComparison()).ToArray(),
            Criteria.Where(c => c.IsMandatory).Select(c => c.Kind).ToArray());
    }

    private async Task RunAsync()
    {
        var errors = new List<string>();
        var request = TryBuildRequest(errors);
        if (request is not null) errors.AddRange(SimulationRequestValidator.Validate(request));
        if (!IsConsistent)
            errors.Add("As comparações entre critérios são inconsistentes (RC > 0,10). Reveja-as antes de executar.");
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

    private async Task LoadDefaultsAsync()
    {
        try
        {
            ApplyParameters(await _service.GetDefaultParametersAsync());
            StatusMessage = "Parâmetros predefinidos repostos.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível ler os parâmetros predefinidos.";
        }
    }

    private async Task SaveDefaultsAsync()
    {
        var errors = new List<string>();
        var parameters = TryReadParameters(errors);
        if (parameters is not null) errors.AddRange(SimulationRequestValidator.ValidateParameters(parameters));
        SetErrors(errors);
        if (parameters is null || errors.Count > 0)
        {
            StatusMessage = "Corrija os parâmetros antes de os guardar como predefinição.";
            return;
        }

        try
        {
            await _service.SaveDefaultParametersAsync(parameters);
            StatusMessage = "Parâmetros guardados como predefinição para novas simulações.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível guardar os parâmetros predefinidos.";
        }
    }

    private async Task FactoryDefaultsAsync()
    {
        try
        {
            ApplyParameters(await _service.ResetDefaultParametersAsync());
            StatusMessage = "Foram repostos os valores de origem da aplicação.";
        }
        catch (Exception)
        {
            StatusMessage = "Não foi possível repor os valores de origem.";
        }
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
        OnPropertyChanged(nameof(OutcomeConsistencyText));
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
        var parameters = TryReadParameters([]);
        var valid = parameters is not null && SimulationRequestValidator.ValidateParameters(parameters).Count == 0;
        foreach (var park in Parks)
        {
            park.EstimatedMonthlyCost = valid && park.Rent is > 0
                ? $"{NumberInput.Money(_service.EstimateCosts(park.Rent.Value, parameters!).MonthlyTotal)} / mês"
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

    private void ResetComparisons()
    {
        _suspendRecalculation = true;
        try
        {
            foreach (var comparison in Comparisons) comparison.Position = 0;
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
            Comparisons.Where(c => !c.IsNeutral).Select(c => c.ToComparison()),
            Criteria.Where(c => c.IsMandatory).Select(c => c.Kind).ToArray());

        var max = preview.Weights.Max(w => w.Weight);
        foreach (var criterion in Criteria)
        {
            var weight = preview.Weights.Single(w => w.Kind == criterion.Kind).Weight;
            criterion.Weight = weight;
            criterion.BarValue = max > 0 ? (double)(weight / max) : 0;
        }

        var ratio = preview.Consistency.ConsistencyRatio;
        IsConsistent = preview.Consistency.IsAcceptable;
        ConsistencyText = IsConsistent
            ? $"Comparações consistentes (RC = {ratio.ToString("0.000", NumberInput.Portuguese)} ≤ 0,10)."
            : $"Comparações inconsistentes (RC = {ratio.ToString("0.000", NumberInput.Portuguese)} > 0,10). Reveja as preferências.";
    }
}
