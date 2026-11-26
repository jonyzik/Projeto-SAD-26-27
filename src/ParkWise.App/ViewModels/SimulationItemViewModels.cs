using ParkWise.Application.Simulations;
using ParkWise.Core.Enums;

namespace ParkWise.App.ViewModels;

public sealed class SelectableParkViewModel(CandidatePark park, Action selectionChanged) : ViewModelBase
{
    private bool _isSelected;
    private string _estimatedMonthlyCost = "—";

    public Guid Id { get; } = park.Id;
    public string Name { get; } = park.Name;
    public string City { get; } = park.City;
    public decimal? Rent { get; } = park.MonthlyRentPerSquareMetre;
    public string RentText { get; } = park.MonthlyRentPerSquareMetre is > 0
        ? $"{NumberInput.Money(park.MonthlyRentPerSquareMetre)} / m²"
        : "Sem preço";
    public string EvaluationText { get; } =
        $"{park.Scores.Keys.Count(kind => kind != CriterionKind.CompetitiveRentPerSquareMetre)}/8 critérios avaliados";

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value)) selectionChanged();
        }
    }

    public string EstimatedMonthlyCost
    {
        get => _estimatedMonthlyCost;
        set => SetProperty(ref _estimatedMonthlyCost, value);
    }
}

/// <summary>Um critério de avaliação com a importância (1 a 9) indicada pelo utilizador e o peso calculado pelo AHP.</summary>
public sealed class CriterionPreferenceViewModel(CriterionKind kind, string name, Action changed) : ViewModelBase
{
    private int _importance = ImportanceScale.Default;
    private bool _isMandatory;
    private decimal _weight;
    private double _barValue;

    public CriterionKind Kind { get; } = kind;
    public string Name { get; } = name;

    /// <summary>Importância de 1 a 9; o seletor usa double, por isso o valor é arredondado.</summary>
    public double Importance
    {
        get => _importance;
        set
        {
            var rounded = (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), ImportanceScale.Minimum, ImportanceScale.Maximum);
            if (!SetProperty(ref _importance, rounded)) return;
            OnPropertyChanged(nameof(ImportanceText));
            changed();
        }
    }

    public int ImportanceValue => _importance;
    public string ImportanceText => $"{_importance} · {ImportanceScale.Describe(_importance)}";

    /// <summary>O preço é sempre calculado; não pode ser marcado como obrigatório.</summary>
    public bool CanBeMandatory => Kind != CriterionKind.CompetitiveRentPerSquareMetre;

    public bool IsMandatory
    {
        get => _isMandatory;
        set
        {
            if (SetProperty(ref _isMandatory, value && CanBeMandatory)) changed();
        }
    }

    public decimal Weight
    {
        get => _weight;
        set
        {
            if (!SetProperty(ref _weight, value)) return;
            OnPropertyChanged(nameof(WeightText));
        }
    }

    public string WeightText => NumberInput.Percent(Weight);

    /// <summary>Valor relativo ao maior peso (0 a 1) para o gráfico de barras.</summary>
    public double BarValue
    {
        get => _barValue;
        set => SetProperty(ref _barValue, value);
    }
}

public sealed class ResultRowViewModel
{
    public ResultRowViewModel(ParkSimulationResult result, decimal maxScore)
    {
        Result = result;
        RankText = result.Ranking is int rank ? $"{rank}.º" : "—";
        ScoreText = result.IsEligible ? NumberInput.Percent(result.Score) : "—";
        ScoreBar = maxScore > 0 && result.IsEligible ? (double)(result.Score / maxScore) : 0;
        MonthlyCostText = NumberInput.Money(result.Costs?.Monthly);
        ContractCostText = NumberInput.Money(result.Costs?.ContractTotal);
        StatusText = result.IsEligible ? "Elegível" : string.Join(" ", result.ExclusionReasons);
        StrengthsText = result.Strengths.Count > 0 ? string.Join(" · ", result.Strengths) : "—";
        WeaknessesText = result.Weaknesses.Count > 0 ? string.Join(" · ", result.Weaknesses) : "—";
        CostLines =
        [
            new("Custo mensal (área × preço/m²)", NumberInput.Money(result.Costs?.Monthly)),
            new("Custo anual", NumberInput.Money(result.Costs?.Annual)),
            new("Custo total do contrato", NumberInput.Money(result.Costs?.ContractTotal))
        ];

        var maxContribution = result.Contributions.Count > 0 ? result.Contributions.Max(c => c.WeightedContribution) : 0;
        Contributions = result.Contributions
            .OrderByDescending(c => c.WeightedContribution)
            .Select(c => new ContributionLine(c.Name, NumberInput.Percent(c.WeightedContribution, 2),
                maxContribution > 0 ? (double)(c.WeightedContribution / maxContribution) : 0))
            .ToArray();
    }

    public ParkSimulationResult Result { get; }
    public string RankText { get; }
    public string ParkName => Result.ParkName;
    public string City => Result.City;
    public bool IsEligible => Result.IsEligible;
    public string ScoreText { get; }
    public double ScoreBar { get; }
    public string MonthlyCostText { get; }
    public string ContractCostText { get; }
    public string StatusText { get; }
    public string StrengthsText { get; }
    public string WeaknessesText { get; }
    public IReadOnlyList<LabelValue> CostLines { get; }
    public IReadOnlyList<ContributionLine> Contributions { get; }
    public bool HasContributions => Contributions.Count > 0;
}

public sealed record LabelValue(string Label, string Value);
public sealed record ContributionLine(string Name, string Value, double Bar);
