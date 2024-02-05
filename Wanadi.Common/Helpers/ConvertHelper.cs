using System.Globalization;

namespace Wanadi.Common.Helpers;

public static class ConvertHelper
{
    public static DateTime ToDateTime(string value)
        => ToDateTime(value, "dd/MM/yyyy");

    public static DateTime ToDateTime(string value, string format)
    {
        if (string.IsNullOrEmpty(value))
            return DateTime.MinValue;

        if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var convertedValue))
            return convertedValue;

        return DateTime.MinValue;
    }

    public static Int32 ToInt32(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        if (Int32.TryParse(value, out var convertedVale))
            return convertedVale;

        return 0;
    }

    public static Decimal ToDecimal(string value)
    {
        if (string.IsNullOrEmpty(value))
            return decimal.Zero;

        if (Decimal.TryParse(value, out var convertedValue))
            return convertedValue;

        return decimal.Zero;
    }

    public static Decimal ToDecimal(string value, bool reviseCulture)
    {
        if (string.IsNullOrEmpty(value))
            return decimal.Zero;

        if (!reviseCulture)
            return ToDecimal(value);

        if (value.Contains(",") && value.Contains(".") && value.IndexOf(",") < value.IndexOf("."))
            return ToDecimal(value, new CultureInfo("en-US"));

        if (value.Contains(",") && value.Contains(".") && value.IndexOf(",") > value.IndexOf("."))
            return ToDecimal( value.Replace(".", string.Empty), new CultureInfo("pt-BR"));

        if (value.Contains("."))
            return ToDecimal(value, new CultureInfo("en-US"));

        return ToDecimal(value, new CultureInfo("pt-BR"));
    }

    public static Decimal ToDecimal(string value, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(value))
            return decimal.Zero;

        NumberStyles styleNumber = NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;

        if (Decimal.TryParse(value, styleNumber, cultureInfo, out var convertedValue))
            return convertedValue;

        return decimal.Zero;
    }
}