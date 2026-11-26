using ParkWise.Core.Enums;

namespace ParkWise.Core.AHP;

public static class AHPCriteria
{
    public static IReadOnlyList<(CriterionKind Kind, string Name)> Required { get; } =
    [
        (CriterionKind.GeographicLocation, "Localização preferida (cidade)"),
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