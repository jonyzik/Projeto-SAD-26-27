using System.Windows.Input;
using ParkWise.Application.AhpConfiguration;
using ParkWise.Core;

namespace ParkWise.App.ViewModels;

public sealed class AhpCriteriaConfigurationViewModel : ViewModelBase
{
    private readonly AhpComparisonSession _session = new(EvaluationCriteria.Required);
    private bool _isReviewing;
    private AhpResultPresentation _result = AhpResultPresentation.Awaiting;
    private IReadOnlyList<AhpReviewRow> _reviewRows = [];

    public AhpCriteriaConfigurationViewModel()
    {
        PreviousCommand = new RelayCommand(_ => { _session.MovePrevious(); RefreshQuestion(); });
        NextCommand = new RelayCommand(_ => { _session.MoveNext(); RefreshQuestion(); });
        FirstIncompleteCommand = new RelayCommand(_ => { _session.GoToFirstIncomplete(); IsReviewing = false; RefreshQuestion(); });
        ClearAnswerCommand = new RelayCommand(_ => { _session.ClearCurrent(); RefreshQuestion(); });
        ChooseDirectionCommand = new RelayCommand(parameter =>
        {
            if (parameter is string text && Enum.TryParse<AhpPreferenceDirection>(text, out var direction))
                SelectDirection(direction);
        });
        SelectIntensityCommand = new RelayCommand(parameter =>
        {
            if (!CanChooseIntensity || parameter is not int intensity) return;
            _session.SelectIntensity(intensity);
            RefreshQuestion();
        });
        AdvanceCommand = new RelayCommand(_ => AdvanceToReview());
        EditCommand = new RelayCommand(_ => IsReviewing = false);
    }

    public IReadOnlyList<SaatyScaleOption> ScaleOptions { get; } =
    [
        new(2, "2 — Intermédio entre igual e moderada"),
        new(3, "3 — Importância moderada"),
        new(4, "4 — Intermédio entre moderada e forte"),
        new(5, "5 — Importância forte"),
        new(6, "6 — Intermédio entre forte e muito forte"),
        new(7, "7 — Importância muito forte"),
        new(8, "8 — Intermédio entre muito forte e extrema"),
        new(9, "9 — Importância extrema")
    ];

    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand FirstIncompleteCommand { get; }
    public ICommand ClearAnswerCommand { get; }
    public ICommand ChooseDirectionCommand { get; }
    public ICommand SelectIntensityCommand { get; }
    public ICommand AdvanceCommand { get; }
    public ICommand EditCommand { get; }

    public string LeftCriterionName => _session.CurrentQuestion.LeftName;
    public string RightCriterionName => _session.CurrentQuestion.RightName;
    public string StepText => $"Comparação {_session.CurrentIndex + 1} de {_session.Questions.Count}";
    public string ProgressText => $"{_session.AnsweredCount} de {_session.Questions.Count} comparações concluídas";
    public int AnsweredCount => _session.AnsweredCount;
    public int TotalCount => _session.Questions.Count;
    public bool CanGoPrevious => _session.CurrentIndex > 0;
    public bool CanGoNext => _session.CurrentIndex < _session.Questions.Count - 1;
    public bool CanAdvance => _session.CanAdvance;
    public bool CanChooseIntensity => _session.CurrentDraft.Direction is AhpPreferenceDirection.Left or AhpPreferenceDirection.Right;
    public string AnswerStatusText => _session.CurrentDraft switch
    {
        { IsComplete: true, Direction: AhpPreferenceDirection.Equal } => "Resposta guardada: igual importância.",
        { IsComplete: true, Direction: AhpPreferenceDirection.Left } => "Resposta guardada: critério da esquerda mais importante.",
        { IsComplete: true, Direction: AhpPreferenceDirection.Right } => "Resposta guardada: critério da direita mais importante (comparação recíproca).",
        { Direction: AhpPreferenceDirection.Left or AhpPreferenceDirection.Right } => "Escolha a intensidade de 2 a 9 para concluir esta comparação.",
        _ => "Por responder. Pode avançar e voltar mais tarde."
    };

    public bool PreferEqual => _session.CurrentDraft.Direction == AhpPreferenceDirection.Equal;
    public bool PreferLeft => _session.CurrentDraft.Direction == AhpPreferenceDirection.Left;
    public bool PreferRight => _session.CurrentDraft.Direction == AhpPreferenceDirection.Right;

    public bool IsReviewing
    {
        get => _isReviewing;
        private set
        {
            if (!SetProperty(ref _isReviewing, value)) return;
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
            if (!SetProperty(ref _result, value)) return;
            OnPropertyChanged(nameof(HasWeights));
            OnPropertyChanged(nameof(HasResultError));
            OnPropertyChanged(nameof(ResultStateText));
        }
    }

    public bool HasWeights => Result.Weights.Count > 0;
    public bool HasResultError => Result.State == AhpResultDisplayState.Error;
    public string ResultStateText => Result.State switch
    {
        AhpResultDisplayState.Available => "Resultados disponíveis",
        AhpResultDisplayState.Inconsistent => "A consistência requer revisão",
        AhpResultDisplayState.Error => "Ocorreu um erro no cálculo",
        _ => "A aguardar integração com o motor AHP"
    };

    public void ShowResult(AhpResultPresentation presentation) => Result = presentation;

    public IReadOnlyList<AhpPairwiseResponse> GetCompletedResponses() =>
        _session.ExportCompletedResponses();

    private void SelectDirection(AhpPreferenceDirection direction)
    {
        if (_session.CurrentDraft.Direction == direction) return;
        _session.SelectDirection(direction);
        RefreshQuestion();
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
        OnPropertyChanged(nameof(CanChooseIntensity));
        OnPropertyChanged(nameof(AnswerStatusText));
        OnPropertyChanged(nameof(PreferEqual));
        OnPropertyChanged(nameof(PreferLeft));
        OnPropertyChanged(nameof(PreferRight));
        foreach (var option in ScaleOptions)
            option.SetSelected(option.Intensity == _session.CurrentDraft.Intensity && CanChooseIntensity);
    }

    private void AdvanceToReview()
    {
        if (!_session.CanAdvance) return;
        var responses = _session.ExportCompletedResponses();
        ReviewRows = responses.Select((response, index) =>
        {
            var question = _session.Questions[index];
            var choice = response.Direction switch
            {
                AhpPreferenceDirection.Equal => "1 · Igual importância",
                AhpPreferenceDirection.Left => $"{response.Intensity} · {question.LeftName}",
                _ => $"{question.RightName} · recíproca 1/{response.Intensity}"
            };
            return new AhpReviewRow(question.LeftName, question.RightName, choice);
        }).ToArray();
        IsReviewing = true;
    }
}
