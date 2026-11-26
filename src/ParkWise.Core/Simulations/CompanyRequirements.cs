namespace ParkWise.Core.Simulations;

/// <summary>
/// Dados da empresa indicados numa simulação para calcular os custos operacionais:
/// a área a arrendar e a duração do contrato.
/// </summary>
public sealed class CompanyRequirements
{
    public decimal RequiredAreaSquareMetres { get; set; }
    public int ContractDurationMonths { get; set; }
}
