namespace ParkWise.Core.Simulations;

/// <summary>
/// Necessidades da empresa indicadas numa simulação. São usadas para calcular
/// os custos operacionais e para excluir parques que não as satisfazem.
/// </summary>
public sealed class CompanyRequirements
{
    public decimal RequiredAreaSquareMetres { get; set; }
    public int EmployeeCount { get; set; }
    public int ContractDurationMonths { get; set; }
    public decimal? MaxMonthlyBudget { get; set; }
    public string? PreferredCity { get; set; }
}
