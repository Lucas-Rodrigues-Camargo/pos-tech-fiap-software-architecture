using System.Text.RegularExpressions;

namespace OficinaMecanica.Application.Common;

public static class DocumentValidator
{
    public static string Normalize(string value) => Regex.Replace(value ?? string.Empty, "[^0-9A-Za-z]", string.Empty).ToUpperInvariant();

    public static bool IsValidCpfOrCnpj(string document)
    {
        var normalized = Normalize(document);
        return normalized.Length is 11 or 14 && normalized.All(char.IsDigit);
    }

    public static bool IsValidPlate(string plate)
    {
        var normalized = Normalize(plate);
        return Regex.IsMatch(normalized, "^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$");
    }
}
