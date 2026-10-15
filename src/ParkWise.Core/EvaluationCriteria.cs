using ParkWise.Core.Enums;

namespace ParkWise.Core;

public static class EvaluationCriteria
{
    public static IReadOnlyList<(CriterionKind Kind, string Name)> Required { get; } =
    [
        (CriterionKind.Receptionist, "Rececionista"),
        (CriterionKind.Wifi, "Rede Wi-Fi"),
        (CriterionKind.MeetingRooms, "Salas para reuniões"),
        (CriterionKind.Parking, "Parque de estacionamento"),
        (CriterionKind.PublicTransport, "Transportes públicos"),
        (CriterionKind.CanteenOrRestaurant, "Cantina ou restaurante"),
        (CriterionKind.LeisureArea, "Área de lazer"),
        (CriterionKind.AirConditioning, "Climatização nas salas"),
        (CriterionKind.CompetitiveRentPerSquareMetre, "Preço competitivo da área alugada, por m²")
    ];
}
