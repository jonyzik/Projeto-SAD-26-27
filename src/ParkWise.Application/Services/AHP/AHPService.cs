
namespace ParkWise.Application.Services.AHP;

public class AHPService
{
    private static readonly double[] RandomIndex =
    {
        0, 0, 0.58, 0.90, 1.12,
        1.24, 1.32, 1.41, 1.45, 1.49
    };

    public AHPResult CalculateWeights(double[,] matrix)
    {
        int n = matrix.GetLength(0);

        ValidateMatrix(matrix);

        // Vetor inicial
        double[] weights = Enumerable
            .Repeat(1.0 / n, n)
            .ToArray();

        // Método das potências
        for (int iteration = 0; iteration < 1000; iteration++)
        {
            double[] newWeights = Multiply(matrix, weights);

            double sum = newWeights.Sum();

            for (int i = 0; i < n; i++)
            {
                newWeights[i] /= sum;
            }

            double difference = 0;

            for (int i = 0; i < n; i++)
            {
                difference = Math.Max(
                    difference,
                    Math.Abs(newWeights[i] - weights[i])
                );
            }

            weights = newWeights;

            if (difference < 0.0000001)
                break;
        }

        // Calcular o maior valor próprio
        double[] product = Multiply(matrix, weights);

        double lambdaMax = 0;

        for (int i = 0; i < n; i++)
        {
            lambdaMax += product[i] / weights[i];
        }

        lambdaMax /= n;

        // Índice de consistência
        double ci = n <= 1
            ? 0
            : Math.Max(0, (lambdaMax - n) / (n - 1));

        // Rácio de consistência
        double ri = n <= 10
            ? RandomIndex[n - 1]
            : throw new ArgumentException(
                "Esta implementação suporta até 10 critérios."
            );

        double cr = ri == 0 ? 0 : ci / ri;

        return new AHPResult
        {
            Weights = weights,
            LambdaMax = lambdaMax,
            ConsistencyIndex = ci,
            ConsistencyRatio = cr
        };
    }

    private static double[] Multiply(
        double[,] matrix,
        double[] vector)
    {
        int n = vector.Length;

        double[] result = new double[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }

    private static void ValidateMatrix(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        if (rows != columns || rows == 0)
            throw new ArgumentException(
                "A matriz tem de ser quadrada e não vazia."
            );

        for (int i = 0; i < rows; i++)
        {
            if (Math.Abs(matrix[i, i] - 1) > 0.000001)
                throw new ArgumentException(
                    "A diagonal principal tem de conter valores 1."
                );

            for (int j = 0; j < columns; j++)
            {
                double value = matrix[i, j];

                if (value <= 0 || value < 1.0 / 9.0 || value > 9)
                    throw new ArgumentException(
                        "Os valores têm de estar entre 1/9 e 9."
                    );

                if (Math.Abs(matrix[i, j] * matrix[j, i] - 1)
                    > 0.000001)
                {
                    throw new ArgumentException(
                        "A matriz tem de ser recíproca."
                    );
                }
            }
        }
    }
}
