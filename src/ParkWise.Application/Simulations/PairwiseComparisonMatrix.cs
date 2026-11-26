using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

/// <summary>
/// Constrói a matriz de comparação par a par do AHP a partir da importância (1 a 9) de cada critério.
/// Cada elemento é a razão entre as importâncias: a_ij = importância_i / importância_j.
/// Com notas de 1 a 9 todas as razões ficam dentro da escala de Saaty (1/9 a 9) e a matriz é recíproca
/// e perfeitamente consistente.
/// </summary>
public static class PairwiseComparisonMatrix
{
    public static double[,] FromImportance(IReadOnlyList<CriterionKind> criteria, IReadOnlyDictionary<CriterionKind, int> importance)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentNullException.ThrowIfNull(importance);
        var values = criteria
            .Select(kind => importance.TryGetValue(kind, out var value) ? value : ImportanceScale.Default)
            .ToArray();
        if (values.Any(value => !ImportanceScale.IsValid(value)))
            throw new ArgumentOutOfRangeException(nameof(importance), "A importância de cada critério deve estar entre 1 e 9.");

        var n = values.Length;
        var matrix = new double[n, n];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            matrix[i, j] = (double)values[i] / values[j];
        return matrix;
    }
}
