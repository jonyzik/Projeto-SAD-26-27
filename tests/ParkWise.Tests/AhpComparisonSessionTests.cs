using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.AhpConfiguration;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.Tests;

[TestClass]
public sealed class AhpComparisonSessionTests
{
    [TestMethod]
    public void CreatesThirtySixUniquePairsFromSharedRequiredCriteria()
    {
        var session = CreateSession();

        Assert.AreEqual(36, session.Questions.Count);
        Assert.AreEqual(36, session.Questions.Select(pair => (pair.LeftKind, pair.RightKind)).Distinct().Count());
        Assert.AreEqual(EvaluationCriteria.Required[0].Name, session.CurrentQuestion.LeftName);
        Assert.AreEqual(EvaluationCriteria.Required[1].Name, session.CurrentQuestion.RightName);
        CollectionAssert.AreEquivalent(
            EvaluationCriteria.Required.Select(criterion => criterion.Kind).ToArray(),
            session.Questions.SelectMany(pair => new[] { pair.LeftKind, pair.RightKind }).Distinct().ToArray());
    }

    [TestMethod]
    public void NavigationPreservesCompleteAndPartialChoices()
    {
        var session = CreateSession();
        session.SelectDirection(AhpPreferenceDirection.Left);
        Assert.AreEqual(0, session.AnsweredCount);

        session.MoveNext();
        session.SelectDirection(AhpPreferenceDirection.Equal);
        Assert.AreEqual(1, session.AnsweredCount);

        session.MovePrevious();
        Assert.AreEqual(AhpPreferenceDirection.Left, session.CurrentDraft.Direction);
        Assert.IsNull(session.CurrentDraft.Intensity);
        session.SelectIntensity(4);
        Assert.AreEqual(2, session.AnsweredCount);

        session.MoveNext();
        Assert.AreEqual(AhpPreferenceDirection.Equal, session.CurrentDraft.Direction);
        Assert.AreEqual(1, session.CurrentDraft.Intensity);
        session.MovePrevious();
        Assert.AreEqual(4, session.CurrentDraft.Intensity);
    }

    [TestMethod]
    public void ValidatesScaleAndReciprocalDirectionWithoutCalculatingRatios()
    {
        var session = CreateSession();
        Assert.Throws<InvalidOperationException>(() => session.SelectIntensity(3));

        session.SelectDirection(AhpPreferenceDirection.Right);
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SelectIntensity(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SelectIntensity(10));
        session.SelectIntensity(9);

        Assert.IsTrue(session.CurrentDraft.IsComplete);
        Assert.AreEqual(AhpPreferenceDirection.Right, session.CurrentDraft.Direction);
        Assert.AreEqual(9, session.CurrentDraft.Intensity);

        session.SelectDirection(AhpPreferenceDirection.Left);
        Assert.AreEqual(9, session.CurrentDraft.Intensity);
        session.SelectDirection(AhpPreferenceDirection.Equal);
        Assert.AreEqual(1, session.CurrentDraft.Intensity);
    }

    [TestMethod]
    public void BlocksExportUntilAllComparisonsAreComplete()
    {
        var session = CreateSession();
        for (var index = 0; index < 35; index++)
        {
            session.SelectDirection(AhpPreferenceDirection.Equal);
            session.MoveNext();
        }

        Assert.AreEqual(35, session.AnsweredCount);
        Assert.IsFalse(session.CanAdvance);
        Assert.Throws<InvalidOperationException>(() => session.ExportCompletedResponses());

        session.SelectDirection(AhpPreferenceDirection.Right);
        session.SelectIntensity(3);
        Assert.IsTrue(session.CanAdvance);
        var responses = session.ExportCompletedResponses();
        Assert.AreEqual(36, responses.Count);
        Assert.AreEqual(AhpPreferenceDirection.Right, responses[^1].Direction);
        Assert.AreEqual(3, responses[^1].Intensity);
        Assert.AreEqual(session.Questions[^1].LeftKind, responses[^1].LeftCriterion);
        Assert.AreEqual(session.Questions[^1].RightKind, responses[^1].RightCriterion);
    }

    [TestMethod]
    public void FindsFirstIncompleteAndClearingAnswerUpdatesProgress()
    {
        var session = CreateSession();
        session.SelectDirection(AhpPreferenceDirection.Equal);
        session.MoveNext();
        session.MoveNext();
        session.SelectDirection(AhpPreferenceDirection.Left);
        session.SelectIntensity(2);

        session.GoToFirstIncomplete();
        Assert.AreEqual(1, session.CurrentIndex);
        session.GoTo(2);
        session.ClearCurrent();
        Assert.AreEqual(1, session.AnsweredCount);
        Assert.IsFalse(session.CurrentDraft.IsComplete);
        Assert.Throws<ArgumentOutOfRangeException>(() => session.GoTo(36));
    }

    [TestMethod]
    public void RejectsCriteriaThatDoNotMatchSharedContract()
    {
        var criteria = EvaluationCriteria.Required.ToArray();
        criteria[0] = ((CriterionKind)99, "Outro critério");

        Assert.Throws<ArgumentException>(() => new AhpComparisonSession(criteria));
    }

    private static AhpComparisonSession CreateSession() => new(EvaluationCriteria.Required);
}
