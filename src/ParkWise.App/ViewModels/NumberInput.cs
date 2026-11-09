using System.Globalization;

namespace ParkWise.App.ViewModels;

/// <summary>Leitura e escrita de números aceitando vírgula (pt-PT) ou ponto como separador decimal.</summary>
public static class NumberInput
{
    public static readonly CultureInfo Portuguese = CultureInfo.GetCultureInfo("pt-PT");

    public static bool TryParseDecimal(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var normalized = text.Trim().Replace(" ", string.Empty).Replace(" ", string.Empty)
            .Replace("€", string.Empty).Replace("%", string.Empty);
        if (normalized.Contains(','))
            normalized = normalized.Replace(".", string.Empty).Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseInt(string? text, out int value)
    {
        value = 0;
        return !string.IsNullOrWhiteSpace(text) &&
               int.TryParse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    public static string Format(decimal value) => value.ToString("0.##", Portuguese);

    public static string Money(decimal? value) => value is decimal amount ? $"{amount.ToString("N2", Portuguese)} €" : "—";

    public static string Percent(decimal value, int decimals = 1) =>
        (value * 100m).ToString("N" + decimals, Portuguese) + " %";

    public static string LocalDate(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm", Portuguese);
}
