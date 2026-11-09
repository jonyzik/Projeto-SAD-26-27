using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.Application.AhpConfiguration;

/// <summary>
/// Resposta de entrada para o futuro motor AHP. Right representa a preferência
/// recíproca; esta camada não calcula rácios, matrizes ou pesos.
/// </summary>
public sealed record AhpPairwiseResponse(
    CriterionKind LeftCriterion,
    CriterionKind RightCriterion,
    AhpPreferenceDirection Direction,
    int Intensity);

public enum AhpPreferenceDirection
{
    Equal,
    Left,
    Right
}

public sealed record AhpCriterionPair(
    CriterionKind LeftKind,
    string LeftName,
    CriterionKind RightKind,
    string RightName);

public sealed record AhpComparisonDraft(AhpPreferenceDirection? Direction, int? Intensity)
{
    public bool IsComplete => Direction switch
    {
        AhpPreferenceDirection.Equal => Intensity == 1,
        AhpPreferenceDirection.Left or AhpPreferenceDirection.Right => Intensity is >= 2 and <= 9,
        _ => false
    };
}

/// <summary>Estado temporário da configuração, sem persistência ou cálculos AHP.</summary>
public sealed class AhpComparisonSession
{
    private readonly AhpComparisonDraft[] _drafts;

    public AhpComparisonSession(IReadOnlyList<(CriterionKind Kind, string Name)> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (criteria.Count != EvaluationCriteria.Required.Count ||
            !criteria.Select(criterion => criterion.Kind).Order().SequenceEqual(
                EvaluationCriteria.Required.Select(criterion => criterion.Kind).Order()) ||
            criteria.Any(criterion => string.IsNullOrWhiteSpace(criterion.Name)))
            throw new ArgumentException("A configuração exige nove critérios distintos e com nome.", nameof(criteria));

        var pairs = new List<AhpCriterionPair>(36);
        for (var left = 0; left < criteria.Count; left++)
        for (var right = left + 1; right < criteria.Count; right++)
            pairs.Add(new AhpCriterionPair(
                criteria[left].Kind, criteria[left].Name,
                criteria[right].Kind, criteria[right].Name));

        Questions = pairs.AsReadOnly();
        _drafts = Enumerable.Repeat(new AhpComparisonDraft(null, null), pairs.Count).ToArray();
    }

    public IReadOnlyList<AhpCriterionPair> Questions { get; }
    public int CurrentIndex { get; private set; }
    public AhpCriterionPair CurrentQuestion => Questions[CurrentIndex];
    public AhpComparisonDraft CurrentDraft => _drafts[CurrentIndex];
    public int AnsweredCount => _drafts.Count(draft => draft.IsComplete);
    public bool CanAdvance => AnsweredCount == Questions.Count;

    public void SelectDirection(AhpPreferenceDirection direction)
    {
        if (!Enum.IsDefined(direction)) throw new ArgumentOutOfRangeException(nameof(direction));
        var intensity = direction == AhpPreferenceDirection.Equal
            ? 1
            : CurrentDraft.Intensity is >= 2 and <= 9 ? CurrentDraft.Intensity : null;
        _drafts[CurrentIndex] = new AhpComparisonDraft(direction, intensity);
    }

    public void SelectIntensity(int intensity)
    {
        if (CurrentDraft.Direction is not (AhpPreferenceDirection.Left or AhpPreferenceDirection.Right))
            throw new InvalidOperationException("Escolha primeiro o critério mais importante.");
        if (intensity is < 2 or > 9) throw new ArgumentOutOfRangeException(nameof(intensity));
        _drafts[CurrentIndex] = CurrentDraft with { Intensity = intensity };
    }

    public void ClearCurrent() => _drafts[CurrentIndex] = new AhpComparisonDraft(null, null);

    public void MovePrevious()
    {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    public void MoveNext()
    {
        if (CurrentIndex < Questions.Count - 1) CurrentIndex++;
    }

    public void GoTo(int index)
    {
        if (index < 0 || index >= Questions.Count) throw new ArgumentOutOfRangeException(nameof(index));
        CurrentIndex = index;
    }

    public void GoToFirstIncomplete()
    {
        var index = Array.FindIndex(_drafts, draft => !draft.IsComplete);
        if (index >= 0) CurrentIndex = index;
    }

    public IReadOnlyList<AhpPairwiseResponse> ExportCompletedResponses()
    {
        if (!CanAdvance) throw new InvalidOperationException("Conclua as 36 comparações antes de avançar.");
        return Questions.Select((question, index) => new AhpPairwiseResponse(
            question.LeftKind,
            question.RightKind,
            _drafts[index].Direction!.Value,
            _drafts[index].Intensity!.Value)).ToArray();
    }
}
