namespace ParkWise.Core.AHP;

public class AHPComparisonMatrix
{
    private static readonly double[] SaatyValues =
    [
        1, 2, 3, 4, 5, 6, 7, 8, 9,
        1.0 / 2, 1.0 / 3, 1.0 / 4,
        1.0 / 5, 1.0 / 6, 1.0 / 7,
        1.0 / 8, 1.0 / 9
    ];

    private readonly double[,] _values;
    private readonly bool[,] _isSet;

    public int Size { get; }

    public AHPComparisonMatrix(int size)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(
                nameof(size), "A matriz tem de ter pelo menos um critério.");
        if (size > 15)
            throw new ArgumentOutOfRangeException(
                nameof(size), "Esta implementação suporta até 15 critérios.");

        Size = size;

        _values = new double[size, size];
        _isSet = new bool[size, size];

        for (int i = 0; i < size; i++)
        {
            _values[i, i] = 1.0;
            _isSet[i, i] = true;
        }
    }

    public double this[int row, int column] => _values[row, column];

    public bool IsComplete
    {
        get
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (!_isSet[i, j])
                        return false;
                }
            }

            return true;
        }
    }

    public void SetComparison(int row, int column, double value)
    {
        if (row < 0 || row >= Size)
            throw new ArgumentOutOfRangeException(nameof(row));
        if (column < 0 || column >= Size)
            throw new ArgumentOutOfRangeException(nameof(column));

        if (row == column)
            throw new ArgumentException(
                "Não podes comparar um critério consigo próprio.");

        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentException(
                "O valor da comparação tem de ser um número positivo.");

        if (!IsValidSaatyValue(value))
            throw new ArgumentException(
                "Utiliza a escala de Saaty: 1 a 9 ou os respetivos inversos.");

        _values[row, column] = value;
        _values[column, row] = 1.0 / value;

        _isSet[row, column] = true;
        _isSet[column, row] = true;
    }

    private static bool IsValidSaatyValue(double value)
    {
        return SaatyValues.Any(v =>
            Math.Abs(v - value) < 0.000001);
    }
}