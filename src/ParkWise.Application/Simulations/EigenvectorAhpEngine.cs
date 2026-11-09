using ParkWise.Application.Ahp;

namespace ParkWise.Application.Simulations;

/// <summary>
/// Implementação de referência do AHP usada pelas simulações enquanto o motor do módulo dev/ahp
/// não estiver integrado. Calcula o vetor próprio principal pelo método das potências e a razão de
/// consistência de Saaty. Para usar o motor da equipa basta registar outra implementação de
/// <see cref="IAhpEngine"/> na injeção de dependências.
/// </summary>
public sealed class EigenvectorAhpEngine : IAhpEngine
{
    // Índices aleatórios de Saaty para n = 1..15.
    private static readonly double[] RandomIndex =
        [0, 0, 0.58, 0.90, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49, 1.51, 1.48, 1.56, 1.57, 1.59];

    private const int MaxIterations = 1000;
    private const double Tolerance = 1e-12;

    public AhpPriorityResult CalculatePriorities(double[,] pairwiseMatrix)
    {
        ArgumentNullException.ThrowIfNull(pairwiseMatrix);
        var n = pairwiseMatrix.GetLength(0);
        if (n == 0 || n != pairwiseMatrix.GetLength(1))
            throw new ArgumentException("A matriz de comparação deve ser quadrada e não vazia.", nameof(pairwiseMatrix));

        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            if (!(pairwiseMatrix[i, j] > 0) || double.IsInfinity(pairwiseMatrix[i, j]))
                throw new ArgumentException("A matriz de comparação só pode conter valores positivos.", nameof(pairwiseMatrix));

        var weights = Enumerable.Repeat(1.0 / n, n).ToArray();
        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var next = Multiply(pairwiseMatrix, weights);
            var sum = next.Sum();
            for (var i = 0; i < n; i++) next[i] /= sum;

            var delta = next.Select((value, i) => Math.Abs(value - weights[i])).Max();
            weights = next;
            if (delta < Tolerance) break;
        }

        var product = Multiply(pairwiseMatrix, weights);
        var lambdaMax = product.Select((value, i) => value / weights[i]).Average();
        var ci = n > 1 ? Math.Max(0, (lambdaMax - n) / (n - 1)) : 0;
        var ri = n <= RandomIndex.Length ? RandomIndex[n - 1] : RandomIndex[^1];
        var cr = ri > 0 ? ci / ri : 0;

        return new AhpPriorityResult(weights, lambdaMax, ci, cr);
    }

    public IReadOnlyList<double> Synthesize(IReadOnlyList<double> criteriaWeights, double[,] alternativePriorities)
    {
        ArgumentNullException.ThrowIfNull(criteriaWeights);
        ArgumentNullException.ThrowIfNull(alternativePriorities);
        if (alternativePriorities.GetLength(1) != criteriaWeights.Count)
            throw new ArgumentException("O número de critérios não coincide com o número de pesos.", nameof(alternativePriorities));

        var alternatives = alternativePriorities.GetLength(0);
        var result = new double[alternatives];
        for (var a = 0; a < alternatives; a++)
        for (var c = 0; c < criteriaWeights.Count; c++)
            result[a] += criteriaWeights[c] * alternativePriorities[a, c];
        return result;
    }

    private static double[] Multiply(double[,] matrix, double[] vector)
    {
        var n = vector.Length;
        var result = new double[n];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            result[i] += matrix[i, j] * vector[j];
        return result;
    }
}
