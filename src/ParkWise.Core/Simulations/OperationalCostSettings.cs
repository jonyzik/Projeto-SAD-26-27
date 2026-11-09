namespace ParkWise.Core.Simulations;

/// <summary>
/// Parâmetros usados no cálculo dos custos operacionais de uma simulação.
/// Os valores monetários estão em euros e, salvo indicação, são mensais.
/// </summary>
public sealed class OperationalCostSettings
{
    public decimal ServiceChargePerSquareMetre { get; set; }
    public decimal UtilitiesPerSquareMetre { get; set; }
    public decimal ServicesPerEmployee { get; set; }
    public decimal ParkingSpacesRatio { get; set; }
    public decimal ParkingCostPerSpace { get; set; }
    public decimal SetupCostPerSquareMetre { get; set; }
    public decimal ContingencyPercentage { get; set; }
}
