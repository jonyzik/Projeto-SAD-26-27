namespace ParkWise.Application.Simulations;

/// <summary>
/// Escala de importância de 1 a 9 que o utilizador atribui a cada critério.
/// As notas são convertidas numa matriz de comparação par a par do AHP (a_ij = nota_i / nota_j).
/// </summary>
public static class ImportanceScale
{
    public const int Minimum = 1;
    public const int Maximum = 9;
    public const int Default = 5;

    private static readonly string[] Labels =
    [
        "Mínima", "Muito baixa", "Baixa", "Moderada-baixa", "Moderada",
        "Moderada-alta", "Alta", "Muito alta", "Máxima"
    ];

    public static bool IsValid(int importance) => importance is >= Minimum and <= Maximum;

    public static string Describe(int importance)
    {
        if (!IsValid(importance))
            throw new ArgumentOutOfRangeException(nameof(importance), "A importância deve estar entre 1 e 9.");
        return Labels[importance - 1];
    }
}
