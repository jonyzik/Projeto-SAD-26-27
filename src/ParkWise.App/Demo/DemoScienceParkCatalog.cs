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
            Services(), new(38.7223, -9.1393)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000002"), "Parque Fictício Douro", "Porto",
            "Ambiente de inovação com acesso a transportes e salas partilhadas.", 15.00m,
            Services(CriterionKind.LeisureArea), new(41.1579, -8.6291)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000003"), "Parque Fictício Minho", "Braga",
            "Instalações flexíveis para pequenas empresas e projetos tecnológicos.", 8.75m,
            Services(CriterionKind.CanteenOrRestaurant, CriterionKind.PublicTransport), new(41.5454, -8.4265)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000004"), "Parque Fictício Mondego", "Coimbra",
            "Espaços próximos de centros de investigação e formação.", 10.00m,
            Services(CriterionKind.Parking), new(40.2033, -8.4103)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000005"), "Parque Fictício Sul", "Faro",
            "Núcleo de empresas com serviços essenciais e espaços de reunião.", 18.20m,
            Services(CriterionKind.Receptionist, CriterionKind.LeisureArea), new(37.0194, -7.9304)),
        new(Guid.Parse("30000000-0000-0000-0000-000000000006"), "Parque Fictício Interior", "Viseu",
            "Instalações disponíveis para consulta; preço ainda não indicado.", null,
            Services(CriterionKind.AirConditioning), new(40.6566, -7.9125))
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
