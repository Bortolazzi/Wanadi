using System.Globalization;
using System.Text.RegularExpressions;

namespace Wanadi.Common.Extensions;

public static class StringExtensions
{
    public static string RemoveNotNumeric(this string text)
      => Regex.Replace((text ?? string.Empty), "[^0-9]", "").Trim();

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
}