using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkWise.Application.AhpConfiguration;
using ParkWise.Application.ScienceParks;
using ParkWise.App.ViewModels;
using ParkWise.Core;
using ParkWise.Core.AHP;
using ParkWise.Core.Enums;

namespace ParkWise.App.Tests;

[TestClass]
public sealed class AhpCriteriaConfigurationViewModelTests
{
    [TestMethod]
    public void AnswerSelectionUpdatesProgressAndRestoresWhenNavigating()
    {
        var viewModel = CreateViewModel();
        var answer = viewModel.AnswerOptions.Single(option =>
            option.Direction == AhpPreferenceDirection.Right &&
            option.Intensity == 7);

        viewModel.SelectedAnswer = answer;

        Assert.AreEqual(45, viewModel.TotalCount);
        Assert.AreEqual(1, viewModel.AnsweredCount);
        Assert.IsFalse(viewModel.CanAdvance);
        Assert.AreEqual("Comparação 1 de 45", viewModel.StepText);

        viewModel.NextCommand.Execute(null);

        Assert.IsNull(viewModel.SelectedAnswer);
        Assert.AreEqual("Comparação 2 de 45", viewModel.StepText);

        viewModel.PreviousCommand.Execute(null);

        Assert.AreSame(answer, viewModel.SelectedAnswer);
        Assert.AreEqual(1, viewModel.AnsweredCount);
    }

    [TestMethod]
    public void ClearingAndResettingAnswersUpdatesTheQuestionState()
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedAnswer = viewModel.AnswerOptions[1];
        viewModel.ClearAnswerCommand.Execute(null);

        Assert.IsNull(viewModel.SelectedAnswer);
        Assert.AreEqual(0, viewModel.AnsweredCount);

        viewModel.SetAllEqualCommand.Execute(null);
        Assert.AreEqual(viewModel.TotalCount, viewModel.AnsweredCount);

        viewModel.ResetCommand.Execute(null);

        Assert.IsTrue(viewModel.IsQuestionStep);
        Assert.AreEqual(0, viewModel.AnsweredCount);
        Assert.AreEqual("Comparação 1 de 45", viewModel.StepText);
        Assert.IsNull(viewModel.SelectedAnswer);
    }

    [TestMethod]
    public void EqualAnswersProduceTenEqualWeightsAndAConsistentResult()
    {
        var viewModel = CreateViewModel();
        viewModel.SetAllEqualCommand.Execute(null);

        Assert.IsTrue(viewModel.CanAdvance);
        viewModel.AdvanceCommand.Execute(null);

        Assert.IsTrue(viewModel.IsReviewStep);
        Assert.AreEqual(AhpResultDisplayState.Available, viewModel.Result.State);
        Assert.AreEqual(10, viewModel.Result.Weights.Count);
        Assert.AreEqual(45, viewModel.ReviewRows.Count);
        Assert.IsTrue(viewModel.Result.Weights.All(weight =>
            weight.ValueText == viewModel.Result.Weights[0].ValueText));
        StringAssert.Contains(viewModel.Result.ConsistencyMessage, "CR:");
        StringAssert.Contains(viewModel.Result.ConsistencyMessage, "0,00");
    }

    [TestMethod]
    public void RightPreferenceCreatesReciprocalAnswerAndHigherWeightForRightCriterion()
    {
        var viewModel = CreateViewModel();
        viewModel.SetAllEqualCommand.Execute(null);
        viewModel.SelectedAnswer = viewModel.AnswerOptions.Single(option =>
            option.Direction == AhpPreferenceDirection.Right &&
            option.Intensity == 9);

        viewModel.AdvanceCommand.Execute(null);

        Assert.AreEqual(AhpResultDisplayState.Available, viewModel.Result.State);
        StringAssert.Contains(viewModel.ReviewRows[0].Choice, "recíproca 1/9");
        Assert.IsTrue(
            ParsePercentage(viewModel.Result.Weights[1].ValueText) >
            ParsePercentage(viewModel.Result.Weights[0].ValueText));
    }

    [TestMethod]
    public void CyclicPreferencesAreFlaggedAsInconsistent()
    {
        var viewModel = CreateViewModel();
        viewModel.SetAllEqualCommand.Execute(null);
        viewModel.SelectedAnswer = FindAnswer(
            viewModel,
            AhpPreferenceDirection.Left,
            9);
        viewModel.NextCommand.Execute(null);
        viewModel.SelectedAnswer = FindAnswer(
            viewModel,
            AhpPreferenceDirection.Right,
            9);

        for (var index = 0; index < 8; index++)
            viewModel.NextCommand.Execute(null);

        viewModel.SelectedAnswer = FindAnswer(
            viewModel,
            AhpPreferenceDirection.Left,
            9);
        viewModel.AdvanceCommand.Execute(null);

        Assert.AreEqual(AhpResultDisplayState.Inconsistent, viewModel.Result.State);
        StringAssert.Contains(viewModel.ReviewRows[1].Choice, "recíproca 1/9");
    }

    [TestMethod]
    public void ReviewCommandDoesNotAdvanceWhileQuestionsAreIncomplete()
    {
        var viewModel = CreateViewModel();

        viewModel.AdvanceCommand.Execute(null);

        Assert.IsTrue(viewModel.IsQuestionStep);
        Assert.AreEqual(AhpResultDisplayState.AwaitingEngine, viewModel.Result.State);
        Assert.AreEqual(0, viewModel.Result.Weights.Count);
    }

    [TestMethod]
    public async Task CalculatesAndDisplaysRankingForSelectedParks()
    {
        var lisbonPark = Park("Parque de Lisboa", "Lisboa", 10m);
        var portoPark = Park("Parque do Porto", "Porto", 20m);
        var incompletePark = Park("Parque sem preço", "Coimbra", null);
        var selection = new ScienceParkSelection();
        selection.SetSelected(lisbonPark.Id, true);
        selection.SetSelected(portoPark.Id, true);
        selection.SetSelected(incompletePark.Id, true);
        var viewModel = CreateViewModel(
            selection,
            lisbonPark,
            portoPark,
            incompletePark);

        await viewModel.LoadAsync();

        Assert.AreEqual(3, viewModel.PreferredCityOptions.Count);
        viewModel.SetAllEqualCommand.Execute(null);
        Assert.IsFalse(viewModel.CanAdvance);
        viewModel.AdvanceCommand.Execute(null);
        Assert.IsTrue(viewModel.IsQuestionStep);

        viewModel.PreferredCity = "Lisboa";
        Assert.IsTrue(viewModel.CanAdvance);
        viewModel.AdvanceCommand.Execute(null);

        Assert.AreEqual(2, viewModel.ParkRanking.Count);
        Assert.AreEqual("Parque de Lisboa", viewModel.ParkRanking[0].Name);
        Assert.AreEqual(100, ParsePercentage(viewModel.ParkRanking[0].ScoreText), 0.01);
        Assert.AreEqual("Parque do Porto", viewModel.ParkRanking[1].Name);
        Assert.AreEqual(80, ParsePercentage(viewModel.ParkRanking[1].ScoreText), 0.01);
        Assert.AreEqual(1, viewModel.UnrankedParks.Count);
        StringAssert.Contains(viewModel.UnrankedParks[0].Reason, "Preço competitivo");
        StringAssert.Contains(viewModel.ParkRanking[0].Criteria[0].DetailText, "Peso");
    }

    [TestMethod]
    public async Task InconsistentCriteriaDoNotProduceParkRanking()
    {
        var parks = new[]
        {
            Park("Parque A", "Lisboa", 10m),
            Park("Parque B", "Lisboa", 15m)
        };
        var selection = new ScienceParkSelection();
        foreach (var park in parks)
            selection.SetSelected(park.Id, true);

        var viewModel = CreateViewModel(selection, parks);
        await viewModel.LoadAsync();
        viewModel.SetAllEqualCommand.Execute(null);
        viewModel.SelectedAnswer = FindAnswer(
            viewModel,
            AhpPreferenceDirection.Left,
            9);
        viewModel.NextCommand.Execute(null);
        viewModel.SelectedAnswer = FindAnswer(
            viewModel,
            AhpPreferenceDirection.Right,
            9);

        for (var index = 0; index < 8; index++)
            viewModel.NextCommand.Execute(null);

        viewModel.SelectedAnswer = FindAnswer(
            viewModel,
            AhpPreferenceDirection.Left,
            9);
        viewModel.AdvanceCommand.Execute(null);

        Assert.AreEqual(AhpResultDisplayState.Inconsistent, viewModel.Result.State);
        Assert.AreEqual(0, viewModel.ParkRanking.Count);
        StringAssert.Contains(viewModel.RankingMessage, "não são consistentes");
    }

    private static AhpCriteriaConfigurationViewModel CreateViewModel() =>
        CreateViewModel(new ScienceParkSelection());

    private static AhpCriteriaConfigurationViewModel CreateViewModel(
        ScienceParkSelection selection,
        params SciencePark[] parks) =>
        new(
            new AhpCalculator(),
            new TestScienceParkCatalog(parks),
            selection,
            new ScienceParkRankingService());

    private static SciencePark Park(string name, string city, decimal? rent) =>
        new(
            Guid.NewGuid(),
            name,
            city,
            string.Empty,
            rent,
            EvaluationCriteria.Required
                .Where(criterion => criterion.Kind != CriterionKind.CompetitiveRentPerSquareMetre)
                .ToDictionary(criterion => criterion.Kind, _ => true));

    private static AhpCriteriaConfigurationViewModel.AhpAnswerOption FindAnswer(
        AhpCriteriaConfigurationViewModel viewModel,
        AhpPreferenceDirection direction,
        int intensity) =>
        viewModel.AnswerOptions.Single(option =>
            option.Direction == direction &&
            option.Intensity == intensity);

    private static double ParsePercentage(string text)
    {
        var numericText = new string(text
            .Where(character => char.IsDigit(character) || character is ',' or '.')
            .ToArray());

        return double.Parse(
            numericText,
            System.Globalization.CultureInfo.GetCultureInfo("pt-PT"));
    }

    private sealed class TestScienceParkCatalog(IReadOnlyList<SciencePark> parks) : IScienceParkCatalog
    {
        public Task<ScienceParkCatalogData> LoadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new ScienceParkCatalogData(parks, IsDemonstration: true));
        }
    }
}
