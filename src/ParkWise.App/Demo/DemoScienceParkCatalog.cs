using ParkWise.Application.ScienceParks;
using ParkWise.Core;
using ParkWise.Core.Enums;

namespace ParkWise.App.Demo;

/// <summary>Dados exclusivamente fictícios; substituir este registo por IScienceParkCatalog real na integração.</summary>
public sealed class DemoScienceParkCatalog : IScienceParkCatalog
{
    private static readonly IReadOnlyList<SciencePark> Parks =
    [
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"), "Parque Fictício Atlântico", "Lisboa",
            "Espaços de trabalho e serviços de apoio para equipas em crescimento.", 12.50m,
            Services()),
        new(Guid.Parse("30000000-0000-0000-0000-000000000002"), "Parque Fictício Douro", "Porto",
            "Ambiente de inovação com acesso a transportes e salas partilhadas.", 15.00m,
            Services(CriterionKind.LeisureArea)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000003"), "Parque Fictício Minho", "Braga",
            "Instalações flexíveis para pequenas empresas e projetos tecnológicos.", 8.75m,
            Services(CriterionKind.CanteenOrRestaurant, CriterionKind.PublicTransport)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000004"), "Parque Fictício Mondego", "Coimbra",
            "Espaços próximos de centros de investigação e formação.", 10.00m,
            Services(CriterionKind.Parking)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000005"), "Parque Fictício Sul", "Faro",
            "Núcleo de empresas com serviços essenciais e espaços de reunião.", 18.20m,
            Services(CriterionKind.Receptionist, CriterionKind.LeisureArea)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000006"), "Parque Fictício Interior", "Viseu",
            "Instalações disponíveis para consulta; preço ainda não indicado.", null,
            Services(CriterionKind.AirConditioning))
    ];

    public Task<ScienceParkCatalogData> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ScienceParkCatalogData(Parks, IsDemonstration: true));
    }

    private static IReadOnlyDictionary<CriterionKind, bool> Services(params CriterionKind[] unavailable) =>
        EvaluationCriteria.Required
            .Where(criterion => criterion.Kind != CriterionKind.CompetitiveRentPerSquareMetre)
            .ToDictionary(criterion => criterion.Kind, criterion => !unavailable.Contains(criterion.Kind));
}
