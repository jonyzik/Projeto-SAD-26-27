namespace ParkWise.Application.Simulations;

/// <summary>
/// Calcula os custos operacionais de arrendar a área pretendida num parque:
/// <list type="bullet">
/// <item>Custo mensal = área (m²) × preço por m² do parque</item>
/// <item>Custo anual = custo mensal × 12</item>
/// <item>Custo total do contrato = custo mensal × duração do contrato (meses)</item>
/// </list>
/// </summary>
public static class OperationalCostCalculator
{
    public static OperationalCost Calculate(decimal monthlyRentPerSquareMetre, CompanyNeeds needs)
    {
        ArgumentNullException.ThrowIfNull(needs);
        if (monthlyRentPerSquareMetre < 0)
            throw new ArgumentOutOfRangeException(nameof(monthlyRentPerSquareMetre), "O preço por m² não pode ser negativo.");

        var monthly = Math.Round(needs.RequiredAreaSquareMetres * monthlyRentPerSquareMetre, 2, MidpointRounding.AwayFromZero);
        return new OperationalCost(monthly, monthly * 12, monthly * needs.ContractDurationMonths);
    }
}
