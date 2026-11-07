
namespace ParkWise.Application.Services.AHP;

public class AHPResult
{
    public double[] Weights { get; set; } = [];

    public double LambdaMax { get; set; }

    public double ConsistencyIndex { get; set; }

    public double ConsistencyRatio { get; set; }

    public bool IsConsistent => ConsistencyRatio <= 0.10;
}
