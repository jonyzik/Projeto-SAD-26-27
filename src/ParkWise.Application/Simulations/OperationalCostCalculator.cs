namespace ParkWise.Application.Simulations;

/// <summary>
/// Calcula os custos operacionais estimados de instalar a empresa num parque.
/// <list type="bullet">
/// <item>Renda = área × preço por m² do parque</item>
/// <item>Condomínio e serviços comuns = área × valor por m²</item>
/// <item>Energia e água = área × valor por m²</item>
/// <item>Serviços por colaborador = colaboradores × valor por colaborador</item>
/// <item>Estacionamento = ⌈colaboradores × rácio de lugares⌉ × custo por lugar</item>
/// <item>Contingência = percentagem sobre a soma anterior</item>
/// <item>Custo de instalação (único) = área × custo por m²</item>
/// <item>Custo total do contrato = custo mensal × meses + instalação</item>
/// </list>
/// </summary>
public static class OperationalCostCalculator
{
    public static OperationalCostBreakdown Calculate(decimal monthlyRentPerSquareMetre, CompanyNeeds needs, CostParameters costs)
    {
        ArgumentNullException.ThrowIfNull(needs);
        ArgumentNullException.ThrowIfNull(costs);
        if (monthlyRentPerSquareMetre < 0)
            throw new ArgumentOutOfRangeException(nameof(monthlyRentPerSquareMetre), "O preço por m² não pode ser negativo.");

        var area = needs.RequiredAreaSquareMetres;
        var rent = Round(area * monthlyRentPerSquareMetre);
        var serviceCharges = Round(area * costs.ServiceChargePerSquareMetre);
        var utilities = Round(area * costs.UtilitiesPerSquareMetre);
        var employeeServices = Round(needs.EmployeeCount * costs.ServicesPerEmployee);
        var parkingSpaces = (int)Math.Ceiling(needs.EmployeeCount * costs.ParkingSpacesRatio);
        var parking = Round(parkingSpaces * costs.ParkingCostPerSpace);

        var subtotal = rent + serviceCharges + utilities + employeeServices + parking;
        var contingency = Round(subtotal * costs.ContingencyPercentage / 100m);
        var monthly = subtotal + contingency;
        var setup = Round(area * costs.SetupCostPerSquareMetre);

        return new OperationalCostBreakdown(
            Rent: rent,
            ServiceCharges: serviceCharges,
            Utilities: utilities,
            EmployeeServices: employeeServices,
            ParkingSpaces: parkingSpaces,
            Parking: parking,
            Contingency: contingency,
            MonthlyTotal: monthly,
            AnnualTotal: monthly * 12,
            SetupCost: setup,
            ContractTotal: monthly * needs.ContractDurationMonths + setup);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
