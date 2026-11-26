namespace ParkWise.Core.AHP;

public class AhpResult
{
    public IReadOnlyList<double> Weights { get; }

    public double LambdaMax { get; }

    public double ConsistencyIndex { get; }

    public double RandomIndex { get; }

    public double ConsistencyRatio { get; }

    public bool IsConsistent => ConsistencyRatio <= 0.10;

    public AhpResult(
        double[] weights,
        double lambdaMax,
        double consistencyIndex,
        double randomIndex,
        double consistencyRatio)
    {
        ArgumentNullException.ThrowIfNull(weights);

        Weights = Array.AsReadOnly((double[])weights.Clone());
        LambdaMax = lambdaMax;
        ConsistencyIndex = consistencyIndex;
        RandomIndex = randomIndex;
        ConsistencyRatio = consistencyRatio;
    }
}