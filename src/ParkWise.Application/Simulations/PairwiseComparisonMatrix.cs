using ParkWise.Core.Enums;

namespace ParkWise.Application.Simulations;

/// <summary>Constrói a matriz recíproca de comparação dos critérios a partir das comparações indicadas.</summary>
public static class PairwiseComparisonMatrix
{
    public static double[,] Build(IReadOnlyList<CriterionKind> criteria, IEnumerable<CriterionComparison> comparisons)
    {
        var n = criteria.Count;
        var index = criteria.Select((kind, i) => (kind, i)).ToDictionary(x => x.kind, x => x.i);
        var matrix = new double[n, n];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            matrix[i, j] = 1.0;

        foreach (var comparison in comparisons)
        {
            if (comparison.Left == comparison.Right) continue;
            if (!index.TryGetValue(comparison.Left, out var i) || !index.TryGetValue(comparison.Right, out var j))
                continue;
            if (!SaatyScale.IsValid(comparison.Value))
                throw new ArgumentOutOfRangeException(nameof(comparisons),
                    $"O valor {comparison.Value} está fora da escala de Saaty (1/9 a 9).");

            matrix[i, j] = (double)comparison.Value;
            matrix[j, i] = 1.0 / (double)comparison.Value;
        }

        return matrix;
    }

    /// <summary>Todos os pares distintos (i &lt; j) de critérios, pela ordem do catálogo.</summary>
    public static IReadOnlyList<(CriterionKind Left, CriterionKind Right)> Pairs(IReadOnlyList<CriterionKind> criteria)
    {
        var pairs = new List<(CriterionKind, CriterionKind)>();
        for (var i = 0; i < criteria.Count; i++)
        for (var j = i + 1; j < criteria.Count; j++)
            pairs.Add((criteria[i], criteria[j]));
        return pairs;
    }
}
