namespace ParkWise.Application.Simulations;

/// <summary>Escala fundamental de Saaty (1 a 9) usada nas comparações par a par do AHP.</summary>
public static class SaatyScale
{
    public const decimal Minimum = 1m / 9m;
    public const decimal Maximum = 9m;

    private static readonly string[] Labels =
    [
        "Igual importância",
        "Igual a moderada",
        "Moderadamente mais importante",
        "Moderada a forte",
        "Fortemente mais importante",
        "Forte a muito forte",
        "Muito fortemente mais importante",
        "Muito forte a extrema",
        "Extremamente mais importante"
    ];

    /// <summary>Descrição da intensidade de um valor inteiro da escala (1 a 9).</summary>
    public static string Describe(int intensity)
    {
        if (intensity is < 1 or > 9)
            throw new ArgumentOutOfRangeException(nameof(intensity), "A intensidade deve estar entre 1 e 9.");
        return Labels[intensity - 1];
    }

    public static bool IsValid(decimal value) =>
        value >= Minimum - 0.0001m && value <= Maximum + 0.0001m;

    /// <summary>
    /// Converte a posição de um seletor simétrico (-8 a 8) num valor de Saaty.
    /// Posições negativas favorecem o critério da esquerda; positivas, o da direita.
    /// </summary>
    public static decimal FromSliderPosition(int position)
    {
        if (position is < -8 or > 8)
            throw new ArgumentOutOfRangeException(nameof(position), "A posição deve estar entre -8 e 8.");
        return position switch
        {
            0 => 1m,
            < 0 => 1 - position,
            _ => 1m / (1 + position)
        };
    }

    /// <summary>Operação inversa de <see cref="FromSliderPosition"/>, arredondada à intensidade inteira mais próxima.</summary>
    public static int ToSliderPosition(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "O valor deve ser positivo.");
        if (value >= 1m) return -(int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero) - 1, 0, 8);
        return (int)Math.Clamp(Math.Round(1m / value, MidpointRounding.AwayFromZero) - 1, 0, 8);
    }
}
