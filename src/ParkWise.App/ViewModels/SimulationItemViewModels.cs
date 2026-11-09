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

public sealed class CriterionPreferenceViewModel(CriterionKind kind, string name, Action mandatoryChanged) : ViewModelBase
{
    private bool _isMandatory;
    private decimal _weight;
    private double _barValue;

    public CriterionKind Kind { get; } = kind;
    public string Name { get; } = name;

    /// <summary>O preço é sempre calculado; não pode ser marcado como obrigatório.</summary>
    public bool CanBeMandatory => Kind != CriterionKind.CompetitiveRentPerSquareMetre;

    public bool IsMandatory
    {
        get => _isMandatory;
        set
        {
            if (SetProperty(ref _isMandatory, value && CanBeMandatory)) mandatoryChanged();
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

public sealed class PairwiseComparisonViewModel(
    CriterionKind left, string leftName, CriterionKind right, string rightName, Action changed) : ViewModelBase
{
    private int _position;

    public CriterionKind Left { get; } = left;
    public CriterionKind Right { get; } = right;
    public string LeftName { get; } = leftName;
    public string RightName { get; } = rightName;

    /// <summary>Posição do seletor: −8 (esquerda extremamente mais importante) a 8 (direita extremamente mais importante).</summary>
    public double Position
    {
        get => _position;
        set
        {
            var rounded = (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), -8, 8);
            if (!SetProperty(ref _position, rounded)) return;
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(IsNeutral));
            changed();
        }
    }

    public bool IsNeutral => _position == 0;

    public decimal Value => SaatyScale.FromSliderPosition(_position);

    public string Description => _position switch
    {
        0 => "Igual importância (1)",
        < 0 => $"{LeftName}: {SaatyScale.Describe(1 - _position).ToLowerInvariant()} ({1 - _position})",
        _ => $"{RightName}: {SaatyScale.Describe(1 + _position).ToLowerInvariant()} ({1 + _position})"
    };

    public CriterionComparison ToComparison() => new(Left, Right, Value);

    public void Load(decimal value) => Position = SaatyScale.ToSliderPosition(value);
}

public sealed class ResultRowViewModel
{
    public ResultRowViewModel(ParkSimulationResult result, decimal maxScore)
    {
        Result = result;
        RankText = result.Ranking is int rank ? $"{rank}.º" : "—";
        ScoreText = result.IsEligible ? NumberInput.Percent(result.Score) : "—";
        ScoreBar = maxScore > 0 && result.IsEligible ? (double)(result.Score / maxScore) : 0;
        MonthlyCostText = NumberInput.Money(result.MonthlyCost);
        ContractCostText = NumberInput.Money(result.ContractCost);
        StatusText = result.IsEligible ? "Elegível" : string.Join(" ", result.ExclusionReasons);
        StrengthsText = result.Strengths.Count > 0 ? string.Join(" · ", result.Strengths) : "—";
        WeaknessesText = result.Weaknesses.Count > 0 ? string.Join(" · ", result.Weaknesses) : "—";

        if (result.CostBreakdown is { } costs)
        {
            CostLines =
            [
                new("Renda", NumberInput.Money(costs.Rent)),
                new("Condomínio e serviços comuns", NumberInput.Money(costs.ServiceCharges)),
                new("Energia e água", NumberInput.Money(costs.Utilities)),
                new("Serviços por colaborador", NumberInput.Money(costs.EmployeeServices)),
                new($"Estacionamento ({costs.ParkingSpaces} lugares)", NumberInput.Money(costs.Parking)),
                new("Contingência", NumberInput.Money(costs.Contingency)),
                new("Total mensal", NumberInput.Money(costs.MonthlyTotal)),
                new("Total anual", NumberInput.Money(costs.AnnualTotal)),
                new("Instalação (valor único)", NumberInput.Money(costs.SetupCost)),
                new("Total do contrato", NumberInput.Money(costs.ContractTotal))
            ];
        }
        else
        {
            CostLines =
            [
                new("Total mensal", NumberInput.Money(result.MonthlyCost)),
                new("Instalação (valor único)", NumberInput.Money(result.SetupCost)),
                new("Total do contrato", NumberInput.Money(result.ContractCost))
            ];
        }

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
