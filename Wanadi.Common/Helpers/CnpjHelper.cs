using System.Text.RegularExpressions;

namespace Wanadi.Common.Helpers;

public static class CnpjHelper
{
    private const int CnpjLengthWithoutCheckDigits = 12;
    private const int AsciiBaseValue = '0';

    private static readonly Regex CnpjRegex = new("^[A-Z0-9]{12}[0-9]{2}$", RegexOptions.Compiled);
    private static readonly Regex DocumentMaskCharactersRegex = new("[./-]", RegexOptions.Compiled);
    private static readonly Regex NotAllowedCnpjCharactersRegex = new("[^A-Z0-9./-]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly int[] CheckDigitWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private const string EmptyCnpj = "00000000000000";

    public static bool IsCnpj(this string? cnpj)
    {
        return IsValidCnpj(cnpj);
    }

    public static bool IsValidCnpj(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj) || NotAllowedCnpjCharactersRegex.IsMatch(cnpj))
            return false;

        var cnpjWithoutMask = RemoveDocumentMask(cnpj);

        if (!CnpjRegex.IsMatch(cnpjWithoutMask) || cnpjWithoutMask == EmptyCnpj)
            return false;

        var informedCheckDigits = cnpjWithoutMask[CnpjLengthWithoutCheckDigits..];
        var calculatedCheckDigits = CalculateCnpjCheckDigits(cnpjWithoutMask[..CnpjLengthWithoutCheckDigits]);

        return informedCheckDigits == calculatedCheckDigits;
    }

    private static string CalculateCnpjCheckDigits(string cnpjWithoutCheckDigits)
    {
        var firstCheckDigitSum = 0;
        var secondCheckDigitSum = 0;

        for (var i = 0; i < CnpjLengthWithoutCheckDigits; i++)
        {
            var asciiDigit = cnpjWithoutCheckDigits[i] - AsciiBaseValue;

            firstCheckDigitSum += asciiDigit * CheckDigitWeights[i + 1];
            secondCheckDigitSum += asciiDigit * CheckDigitWeights[i];
        }

        var firstCheckDigit = CalculateModule11Digit(firstCheckDigitSum);

        secondCheckDigitSum += firstCheckDigit * CheckDigitWeights[CnpjLengthWithoutCheckDigits];

        var secondCheckDigit = CalculateModule11Digit(secondCheckDigitSum);

        return $"{firstCheckDigit}{secondCheckDigit}";
    }

    public static string RemoveDocumentMask(this string cnpj)
    {
        return DocumentMaskCharactersRegex
            .Replace(cnpj, string.Empty)
            .ToUpperInvariant();
    }

    private static int CalculateModule11Digit(int sum)
    {
        var remainder = sum % 11;

        return remainder < 2
            ? 0
            : 11 - remainder;
    }
}