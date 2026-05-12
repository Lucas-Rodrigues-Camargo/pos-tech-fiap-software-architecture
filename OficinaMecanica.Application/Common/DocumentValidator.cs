using System.Text.RegularExpressions;

namespace OficinaMecanica.Application.Common;

public static class DocumentValidator
{
    public static string Normalize(string value) => Regex.Replace(value ?? string.Empty, "[^0-9A-Za-z]", string.Empty).ToUpperInvariant();

    public static bool IsValidCpfOrCnpj(string document)
    {
        var normalized = Normalize(document);
        return normalized.Length switch
        {
            11 => IsValidCpf(normalized),
            14 => IsValidCnpj(normalized),
            _ => false
        };
    }

    public static bool IsValidPlate(string plate)
    {
        var normalized = Normalize(plate);
        return Regex.IsMatch(normalized, "^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$");
    }

    private static bool IsValidCpf(string cpf)
    {
        if (!cpf.All(char.IsDigit) || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var firstDigit = CalculateDigit(cpf, [10, 9, 8, 7, 6, 5, 4, 3, 2]);
        var secondDigit = CalculateDigit(cpf, [11, 10, 9, 8, 7, 6, 5, 4, 3, 2]);

        return cpf[9] == firstDigit && cpf[10] == secondDigit;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (!cnpj.All(char.IsDigit) || cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        var firstDigit = CalculateDigit(cnpj, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        var secondDigit = CalculateDigit(cnpj, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

        return cnpj[12] == firstDigit && cnpj[13] == secondDigit;
    }

    private static char CalculateDigit(string document, int[] weights)
    {
        var sum = 0;
        for (var index = 0; index < weights.Length; index++)
        {
            sum += (document[index] - '0') * weights[index];
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;
        return (char)('0' + digit);
    }
}
