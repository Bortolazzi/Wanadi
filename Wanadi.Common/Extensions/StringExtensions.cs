using System.Globalization;
using System.Text.RegularExpressions;

namespace Wanadi.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Remove via Regex todos os caracteres diferentes de número.
    ///     </para>
    ///     <para>
    ///         en-US: Removes by Regex all characters other than number.
    ///     </para>
    /// </summary>
    /// <param name="text">
    ///     <para>
    ///         pt-BR: String a ser manipulada.
    ///     </para>
    ///     <para>
    ///         en-US: String to be manipulated.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: String contendo apenas números.
    ///     </para>
    ///     <para>
    ///         en-US: String containing only numbers.
    ///     </para>
    /// </returns>
    public static string RemoveNotNumeric(this string text)
      => Regex.Replace((text ?? string.Empty), "[^0-9]", "").Trim();

    /// <summary>
    ///     <para>
    ///         pt-BR: Substitui caracteres com acentuação por caracteres sem acentuação
    ///     </para>
    ///     <para>
    ///         en-US: Replaces accented characters with unaccented characters
    ///     </para>
    /// </summary>
    /// <param name="text">
    ///     <para>
    ///         pt-BR: String a ser manipulada.
    ///     </para>
    ///     <para>
    ///         en-US: String to be manipulated.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: String sem acentuação.
    ///     </para>
    ///     <para>
    ///         en-US: String without accent.
    ///     </para>
    /// </returns>
    public static string RemoveAccents(this string text)
    {
        StringBuilder sbReturn = new StringBuilder();
        var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
        foreach (char letter in arrayText)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                sbReturn.Append(letter);
        }
        return sbReturn.ToString();
    }

    public static string RemoveLeftZeros(this string text)
    {
        while (text.Trim().StartsWith("0"))
            text = text.Trim().Substring(1);

        return text;
    }

    public static string RemoveDoubleSpaces(this string text)
    {
        var value = (text ?? string.Empty);

        while (value.Contains("  "))
            value = value.Replace("  ", " ");

        return value;
    }

    public static string ClearEmptyCharacters(this string text)
    {
        text = (text ?? string.Empty).Replace("\n", " ")
                                     .Replace("\t", " ");

        return text.RemoveDoubleSpaces();
    }
}