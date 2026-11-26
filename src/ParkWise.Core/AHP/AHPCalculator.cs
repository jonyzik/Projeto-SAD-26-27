using System;

namespace ParkWise.Core.AHP;

public class AhpCalculator
{
    // Índices aleatórios de Saaty (RI).
    private static readonly double[] RandomIndexes =
    {
        0.00, // n = 0
        0.00, // n = 1
        0.00, // n = 2
        0.58, // n = 3
        0.90, // n = 4
        1.12, // n = 5
        1.24, // n = 6
        1.32, // n = 7
        1.41, // n = 8
        1.45, // n = 9
        1.49, // n = 10
        1.51, // n = 11
        1.48, // n = 12
        1.56, // n = 13
        1.57, // n = 14
        1.59  // n = 15
    };

    public AhpResult Calculate(AHPComparisonMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        if (!matrix.IsComplete)
        {
            throw new InvalidOperationException(
                "É necessário preencher todas as comparações antes de calcular o AHP.");
        }

        int n = matrix.Size;

        // Médias geométricas das linhas (mínimos quadrados logarítmicos),
        // normalizadas para obter o vetor de prioridades AHP.
        var weights = new double[n];
        var weightSum = 0.0;

        for (var row = 0; row < n; row++)
        {
            var product = 1.0;
            for (var column = 0; column < n; column++)
                product *= matrix[row, column];

            weights[row] = Math.Pow(product, 1.0 / n);
            weightSum += weights[row];
        }

        for (var index = 0; index < n; index++)
            weights[index] /= weightSum;

        // Estimar λmax com o vetor de prioridades para calcular CI e CR.
        double lambdaMax = 0;

        for (int i = 0; i < n; i++)
        {
            double weightedSum = 0;

            for (int j = 0; j < n; j++)
                weightedSum += matrix[i, j] * weights[j];

            lambdaMax += weightedSum / weights[i];
        }

        lambdaMax /= n;

        // Calcular o índice de consistência (CI).
        double consistencyIndex = n <= 1
            ? 0
            : Math.Max(0, (lambdaMax - n) / (n - 1));

        // Obter o índice aleatório (RI).
        double randomIndex = RandomIndexes[n];

        // Calcular o rácio de consistência (CR).
        double consistencyRatio = randomIndex == 0
            ? 0
            : consistencyIndex / randomIndex;

        return new AhpResult(
            weights,
            lambdaMax,
            consistencyIndex,
            randomIndex,
            consistencyRatio);
    }
}