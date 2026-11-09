using System.Globalization;
using ParkWise.Application.ScienceParks;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.App.ViewModels;

public static class ScienceParkPresentation
{
    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-PT");

    public static string Price(decimal? price) => price is > 0
        ? $"{price.Value.ToString("N2", PortugueseCulture)} € / m² / mês"
        : "Preço não indicado";

    public static IReadOnlyList<ScienceParkCriterionRow> Criteria(SciencePark park) =>
        EvaluationCriteria.Required.Select(criterion =>
        {
            if (criterion.Kind == CriterionKind.CompetitiveRentPerSquareMetre)
                return new ScienceParkCriterionRow(criterion.Name, Price(park.MonthlyRentPerSquareMetre));

            return new ScienceParkCriterionRow(criterion.Name,
                park.AvailableServices.TryGetValue(criterion.Kind, out var available)
                    ? available ? "Disponível" : "Não disponível"
                    : "Sem informação");
        }).ToArray();
}

public sealed record ScienceParkCriterionRow(string Name, string Value);

public sealed class ScienceParkCardViewModel : ViewModelBase
{
    private readonly ScienceParkSelection _selection;
    private bool _isSelected;

    public ScienceParkCardViewModel(SciencePark park, ScienceParkSelection selection)
    {
        Park = park;
        _selection = selection;
        _isSelected = selection.Contains(park.Id);
    }

    public SciencePark Park { get; }
    public Guid Id => Park.Id;
    public string Name => Park.Name;
    public string City => Park.City;
    public string Description => Park.Description;
    public string PriceText => ScienceParkPresentation.Price(Park.MonthlyRentPerSquareMetre);
    public IReadOnlyList<ScienceParkCriterionRow> Criteria => ScienceParkPresentation.Criteria(Park);
    public string ServicesText => $"{Park.AvailableServices.Count(service => service.Value)} de 8 serviços disponíveis";

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (!SetProperty(ref _isSelected, value)) return;
            _selection.SetSelected(Id, value);
        }
    }
}
