using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using ParkWise.Application.AhpConfiguration;
using ParkWise.Application.ScienceParks;
using ParkWise.Core.AHP;
using ParkWise.Core.Enums;

namespace ParkWise.App.ViewModels;

public sealed class AhpCriteriaConfigurationViewModel : ViewModelBase
{
    private static readonly CultureInfo PortugueseCulture =
        CultureInfo.GetCultureInfo("pt-PT");

    private readonly AhpComparisonSession _session =
        new(AHPCriteria.Required);

    private readonly AhpCalculator _calculator;
    private readonly IScienceParkCatalog _catalog;
    private readonly ScienceParkSelection _selection;
    private readonly ScienceParkRankingService _rankingService;

    private bool _isReviewing;
    private bool _isLoadingParks;
    private int _selectedParkCount;
    private int _parkLoadVersion;

    private AhpResultPresentation _result =
        AhpResultPresentation.Awaiting;

    private IReadOnlyList<AhpReviewRow> _reviewRows = [];
    private IReadOnlyList<SciencePark> _candidateParks = [];
    private IReadOnlyList<string> _preferredCityOptions = [];
    private IReadOnlyList<ParkRankingDisplay> _parkRanking = [];
    private IReadOnlyList<UnrankedParkDisplay> _unrankedParks = [];
    private AhpAnswerOption? _selectedAnswer;
    private string? _preferredCity;
    private string _rankingMessage =
        "Selecione pelo menos dois parques para gerar um ranking.";

    public AhpCriteriaConfigurationViewModel(
        AhpCalculator calculator,
        IScienceParkCatalog catalog,
        ScienceParkSelection selection,
        ScienceParkRankingService rankingService)
    {
        _calculator = calculator;
        _catalog = catalog;
        _selection = selection;
        _rankingService = rankingService;

        PreviousCommand = new RelayCommand(_ =>
        {
            _session.MovePrevious();
            RefreshQuestion();
        });

        NextCommand = new RelayCommand(_ =>
        {
            _session.MoveNext();
            RefreshQuestion();
        });

        FirstIncompleteCommand = new RelayCommand(_ =>
        {
            _session.GoToFirstIncomplete();
            IsReviewing = false;
            RefreshQuestion();
        });

        ClearAnswerCommand = new RelayCommand(_ =>
        {
            _session.ClearCurrent();
            RefreshQuestion();
        });

        SetAllEqualCommand = new RelayCommand(_ =>
        {
            _session.SetAllEqual();
            IsReviewing = false;
            ClearRanking(GetRankingPrerequisiteMessage());
            RefreshQuestion();
        });

        ResetCommand = new RelayCommand(_ =>
        {
            _session.Reset();
            IsReviewing = false;
            ClearRanking(GetRankingPrerequisiteMessage());
            RefreshQuestion();
        });

        AdvanceCommand = new RelayCommand(_ => AdvanceToReview());

        EditCommand = new RelayCommand(_ =>
        {
            IsReviewing = false;
            ClearRanking(GetRankingPrerequisiteMessage());
        });
    }

    public IReadOnlyList<AhpAnswerOption> AnswerOptions { get; } =
        CreateAnswerOptions();

    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand FirstIncompleteCommand { get; }
    public ICommand ClearAnswerCommand { get; }
    public ICommand SetAllEqualCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand AdvanceCommand { get; }
    public ICommand EditCommand { get; }

    public string LeftCriterionName =>
        _session.CurrentQuestion.LeftName;

    public string RightCriterionName =>
        _session.CurrentQuestion.RightName;

    public string StepText =>
        $"Comparação {_session.CurrentIndex + 1} de {_session.Questions.Count}";

    public string ProgressText =>
        $"{_session.AnsweredCount} de {_session.Questions.Count} comparações concluídas";

    public string CompletionInstructionText =>
        $"É preciso concluir as {_session.Questions.Count} comparações antes de avançar. Pode saltar uma pergunta e voltar depois.";

    public string ReviewHeaderText =>
        $"{_session.Questions.Count} comparações concluídas";

    public bool HasSelectedParks => _candidateParks.Count > 0;

    public bool HasMultipleSelectedParks => _candidateParks.Count >= 2;

    public bool IsLoadingParks
    {
        get => _isLoadingParks;
        private set
        {
            if (SetProperty(ref _isLoadingParks, value))
                OnPropertyChanged(nameof(CanAdvance));
        }
    }

    public string SelectedParksText =>
        _candidateParks.Count == _selectedParkCount
            ? $"{_candidateParks.Count} parque(s) selecionado(s) para avaliação"
            : $"{_candidateParks.Count} de {_selectedParkCount} parques selecionados carregados";

    public IReadOnlyList<string> PreferredCityOptions
    {
        get => _preferredCityOptions;
        private set
        {
            if (SetProperty(ref _preferredCityOptions, value))
            {
                OnPropertyChanged(nameof(CanRankParks));
                OnPropertyChanged(nameof(CanAdvance));
            }
        }
    }

    public string? PreferredCity
    {
        get => _preferredCity;
        set
        {
            if (!SetProperty(ref _preferredCity, value))
                return;

            OnPropertyChanged(nameof(CanRankParks));
            OnPropertyChanged(nameof(CanAdvance));
            ClearRanking(GetRankingPrerequisiteMessage());
        }
    }

    public bool CanRankParks =>
        _candidateParks.Count >= 2 &&
        _candidateParks.Count == _selectedParkCount &&
        !string.IsNullOrWhiteSpace(PreferredCity);

    private bool RequiresPreferredCity =>
        HasMultipleSelectedParks &&
        PreferredCityOptions.Count > 1 &&
        string.IsNullOrWhiteSpace(PreferredCity);

    public string RankingMessage
    {
        get => _rankingMessage;
        private set => SetProperty(ref _rankingMessage, value);
    }

    public IReadOnlyList<ParkRankingDisplay> ParkRanking
    {
        get => _parkRanking;
        private set
        {
            if (!SetProperty(ref _parkRanking, value))
                return;

            OnPropertyChanged(nameof(HasParkRanking));
        }
    }

    public IReadOnlyList<UnrankedParkDisplay> UnrankedParks
    {
        get => _unrankedParks;
        private set
        {
            if (!SetProperty(ref _unrankedParks, value))
                return;

            OnPropertyChanged(nameof(HasUnrankedParks));
        }
    }

    public bool HasParkRanking => ParkRanking.Count > 0;

    public bool HasUnrankedParks => UnrankedParks.Count > 0;

    public AhpAnswerOption? SelectedAnswer
    {
        get => _selectedAnswer;
        set
        {
            if (!SetProperty(ref _selectedAnswer, value))
                return;

            if (value is null)
            {
                _session.ClearCurrent();
            }
            else
            {
                _session.SelectDirection(value.Direction);

                if (value.Direction != AhpPreferenceDirection.Equal)
                    _session.SelectIntensity(value.Intensity);
            }

            RefreshQuestion();
        }
    }

    public string AnswerStatusText => SelectedAnswer is null
        ? "Ainda sem resposta."
        : $"Selecionado: {SelectedAnswer.Label}";

    public int AnsweredCount => _session.AnsweredCount;

    public int TotalCount => _session.Questions.Count;

    public bool CanGoPrevious => _session.CurrentIndex > 0;

    public bool CanGoNext =>
        _session.CurrentIndex < _session.Questions.Count - 1;

    public bool CanAdvance =>
        _session.CanAdvance &&
        !RequiresPreferredCity &&
        !IsLoadingParks;

    public bool IsReviewing
    {
        get => _isReviewing;
        private set
        {
            if (!SetProperty(ref _isReviewing, value))
                return;

            OnPropertyChanged(nameof(IsQuestionStep));
            OnPropertyChanged(nameof(IsReviewStep));
        }
    }

    public bool IsQuestionStep => !IsReviewing;

    public bool IsReviewStep => IsReviewing;

    public IReadOnlyList<AhpReviewRow> ReviewRows
    {
        get => _reviewRows;
        private set => SetProperty(ref _reviewRows, value);
    }

    public AhpResultPresentation Result
    {
        get => _result;
        private set
        {
            if (!SetProperty(ref _result, value))
                return;

            OnPropertyChanged(nameof(HasWeights));
            OnPropertyChanged(nameof(HasResultError));
            OnPropertyChanged(nameof(ResultStateText));
        }
    }

    public bool HasWeights => Result.Weights.Count > 0;

    public bool HasResultError =>
        Result.State == AhpResultDisplayState.Error;

    public string ResultStateText => Result.State switch
    {
        AhpResultDisplayState.Available => "Resultados disponíveis",
        AhpResultDisplayState.Inconsistent => "A consistência requer revisão",
        AhpResultDisplayState.Error => "Ocorreu um erro no cálculo",
        _ => "À espera do cálculo AHP"
    };

    public IReadOnlyList<AhpPairwiseResponse> GetCompletedResponses() =>
        _session.ExportCompletedResponses();

    public async Task LoadAsync()
    {
        var loadVersion = ++_parkLoadVersion;
        var selectedIds = _selection.SelectedIds;
        IsReviewing = false;
        _selectedParkCount = selectedIds.Count;
        _candidateParks = [];
        PreferredCityOptions = [];
        PreferredCity = null;
        IsLoadingParks = _selectedParkCount >= 2;
        ClearRanking(IsLoadingParks
            ? "A carregar os parques selecionados..."
            : GetRankingPrerequisiteMessage());
        NotifyCandidateProperties();

        if (_selectedParkCount < 2)
            return;

        try
        {
            var selectedIdSet = selectedIds.ToHashSet();
            var data = await _catalog.LoadAsync();
            if (loadVersion != _parkLoadVersion)
                return;

            _candidateParks = data.Parks
                .Where(park => selectedIdSet.Contains(park.Id))
                .ToArray();
            PreferredCityOptions = _candidateParks
                .Select(park => park.City.Trim())
                .Where(city => city.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(city => city, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            if (PreferredCityOptions.Count == 1)
                PreferredCity = PreferredCityOptions[0];
            else
                RankingMessage = GetRankingPrerequisiteMessage();

            NotifyCandidateProperties();
        }
        catch (Exception exception)
        {
            if (loadVersion != _parkLoadVersion)
                return;

            Trace.TraceError($"Failed to load parks for AHP ranking: {exception}");
            _candidateParks = [];
            PreferredCityOptions = [];
            RankingMessage = "Não foi possível carregar os parques selecionados para o ranking.";
            NotifyCandidateProperties();
        }
        finally
        {
            if (loadVersion == _parkLoadVersion)
                IsLoadingParks = false;
        }
    }

    private void RefreshQuestion()
    {
        OnPropertyChanged(nameof(LeftCriterionName));
        OnPropertyChanged(nameof(RightCriterionName));
        OnPropertyChanged(nameof(StepText));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(AnsweredCount));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanAdvance));

        var draft = _session.CurrentDraft;
        var selectedAnswer = AnswerOptions.FirstOrDefault(option =>
            option.Direction == draft.Direction &&
            (option.Direction == AhpPreferenceDirection.Equal ||
             option.Intensity == draft.Intensity));

        if (_selectedAnswer != selectedAnswer)
        {
            _selectedAnswer = selectedAnswer;
            OnPropertyChanged(nameof(SelectedAnswer));
        }

        OnPropertyChanged(nameof(AnswerStatusText));
    }

    private void CalculateParkRanking(
        IReadOnlyList<double> weightValues,
        bool isConsistent)
    {
        if (!isConsistent)
        {
            ClearRanking(
                "O ranking está bloqueado porque as comparações AHP não são consistentes. Reveja as respostas e volte a calcular.");
            return;
        }

        if (!CanRankParks)
        {
            ClearRanking(GetRankingPrerequisiteMessage());
            return;
        }

        var weights = AHPCriteria.Required
            .Select((criterion, index) => (criterion.Kind, Weight: weightValues[index]))
            .ToDictionary(item => item.Kind, item => item.Weight);

        var ranking = _rankingService.Rank(
            _candidateParks,
            weights,
            PreferredCity!);

        ParkRanking = ranking.RankedParks
            .Select(entry => new ParkRankingDisplay(
                entry.Rank,
                entry.Park.Name,
                entry.Park.City,
                entry.Score.ToString("P2", PortugueseCulture),
                entry.Criteria.Select(criterion => new ParkCriterionContributionDisplay(
                    criterion.Name,
                    $"Peso {criterion.Weight.ToString("P1", PortugueseCulture)} × " +
                    $"pontuação {criterion.Score.ToString("P0", PortugueseCulture)} = " +
                    criterion.Contribution.ToString("P1", PortugueseCulture)))
                    .ToArray()))
            .ToArray();
        UnrankedParks = ranking.UnrankedParks
            .Select(entry => new UnrankedParkDisplay(
                entry.Park.Name,
                entry.Reason))
            .ToArray();

        RankingMessage = ParkRanking.Count == 0
            ? "Não foi possível classificar pelo menos duas alternativas com os dados disponíveis."
            : UnrankedParks.Count == 0
                ? $"Ranking calculado para {ParkRanking.Count} parques. A pontuação combina os pesos AHP com as pontuações por critério."
                : $"Ranking calculado para {ParkRanking.Count} parques. {UnrankedParks.Count} parque(s) ficaram sem classificação por falta de dados.";
    }

    private string GetRankingPrerequisiteMessage()
    {
        if (_selectedParkCount < 2)
            return "Selecione pelo menos dois parques na pesquisa para gerar um ranking.";

        if (_candidateParks.Count < _selectedParkCount)
            return $"Só foi possível carregar {_candidateParks.Count} de {_selectedParkCount} parques selecionados.";

        if (_candidateParks.Count < 2)
            return "Não foi possível carregar pelo menos dois dos parques selecionados.";

        if (PreferredCityOptions.Count == 0)
            return "Os parques selecionados não têm cidade indicada para avaliar a localização.";

        if (string.IsNullOrWhiteSpace(PreferredCity))
            return "Escolha a cidade preferida para avaliar o critério de localização.";

        return $"{_candidateParks.Count} parques serão avaliados, tendo {PreferredCity} como localização preferida.";
    }

    private void ClearRanking(string message)
    {
        ParkRanking = [];
        UnrankedParks = [];
        RankingMessage = message;
    }

    private void NotifyCandidateProperties()
    {
        OnPropertyChanged(nameof(HasSelectedParks));
        OnPropertyChanged(nameof(HasMultipleSelectedParks));
        OnPropertyChanged(nameof(SelectedParksText));
        OnPropertyChanged(nameof(CanRankParks));
        OnPropertyChanged(nameof(CanAdvance));
    }

    private static IReadOnlyList<AhpAnswerOption> CreateAnswerOptions()
    {
        var options = new List<AhpAnswerOption>
        {
            new("Igual importância (1)", AhpPreferenceDirection.Equal, 1)
        };

        for (var intensity = 2; intensity <= 9; intensity++)
        {
            var label = intensity switch
            {
                2 => "entre igual e moderada",
                3 => "moderada",
                4 => "entre moderada e forte",
                5 => "forte",
                6 => "entre forte e muito forte",
                7 => "muito forte",
                8 => "entre muito forte e extrema",
                9 => "extrema",
                _ => throw new ArgumentOutOfRangeException(nameof(intensity))
            };

            options.Add(new AhpAnswerOption(
                $"Critério A — {intensity} ({label})",
                AhpPreferenceDirection.Left,
                intensity));
            options.Add(new AhpAnswerOption(
                $"Critério B — {intensity} ({label})",
                AhpPreferenceDirection.Right,
                intensity));
        }

        return options.AsReadOnly();
    }

    private void AdvanceToReview()
    {
        if (!CanAdvance)
            return;

        var responses = _session.ExportCompletedResponses();

        ReviewRows = responses.Select((response, index) =>
        {
            var question = _session.Questions[index];

            var choice = response.Direction switch
            {
                AhpPreferenceDirection.Equal =>
                    "1 · Igual importância",

                AhpPreferenceDirection.Left =>
                    $"{response.Intensity} · {question.LeftName}",

                _ =>
                    $"{question.RightName} · recíproca 1/{response.Intensity}"
            };

            return new AhpReviewRow(
                question.LeftName,
                question.RightName,
                choice);
        }).ToArray();

        // Executar os cálculos AHP.
        Result = CalculateResult(responses);

        IsReviewing = true;
    }

    public sealed record AhpAnswerOption(
        string Label,
        AhpPreferenceDirection Direction,
        int Intensity);

    private AhpResultPresentation CalculateResult(
        IReadOnlyList<AhpPairwiseResponse> responses)
    {
        try
        {
            var criteria = AHPCriteria.Required;

            var indexes = criteria
                .Select((criterion, index) => (criterion.Kind, Index: index))
                .ToDictionary(item => item.Kind, item => item.Index);

            var matrix = new AHPComparisonMatrix(criteria.Count);

            foreach (var response in responses)
            {
                double value = response.Direction switch
                {
                    AhpPreferenceDirection.Equal => 1.0,
                    AhpPreferenceDirection.Left => response.Intensity,
                    AhpPreferenceDirection.Right => 1.0 / response.Intensity,
                    _ => throw new InvalidOperationException(
                        "Direção de comparação inválida.")
                };

                matrix.SetComparison(
                    indexes[response.LeftCriterion],
                    indexes[response.RightCriterion],
                    value);
            }

            var calculation = _calculator.Calculate(matrix);
            CalculateParkRanking(calculation.Weights, calculation.IsConsistent);

            var weights = calculation.Weights
                .Select((weight, index) =>
                    new AhpWeightDisplay(
                        criteria[index].Name,
                        weight.ToString("P2", PortugueseCulture)))
                .ToArray();

            var consistencyMessage =
                $"λmax: {calculation.LambdaMax.ToString("F4", PortugueseCulture)}\n" +
                $"CI: {calculation.ConsistencyIndex.ToString("F4", PortugueseCulture)}\n" +
                $"RI: {calculation.RandomIndex.ToString("F2", PortugueseCulture)}\n" +
                $"CR: {calculation.ConsistencyRatio.ToString("P2", PortugueseCulture)}";

            var state = calculation.IsConsistent
                ? AhpResultDisplayState.Available
                : AhpResultDisplayState.Inconsistent;

            var message = calculation.IsConsistent
                ? "A matriz apresenta uma consistência aceitável."
                : "O rácio de consistência ultrapassa 10%. Reveja as comparações e volte a calcular.";

            return new AhpResultPresentation(
                state,
                weights,
                consistencyMessage,
                message);
        }
        catch (Exception exception)
        {
            Trace.TraceError($"Failed to calculate AHP results: {exception}");
            ClearRanking("Não foi possível gerar o ranking com as preferências atuais.");
            return new AhpResultPresentation(
                AhpResultDisplayState.Error,
                [],
                "Não foi possível calcular a consistência da matriz.",
                "Verifique as comparações e tente novamente.");
        }
    }
}