using System.Globalization;
using Wanadi.Common.Helpers;

namespace Wanadi.Common.Extensions;

public static class StringConvertExtensions
{
    public static DateTime ToDateTime(this string value)
        => ConvertHelper.ToDateTime(value);

    public static DateTime ToDateTime(this string value, string format)
        => ConvertHelper.ToDateTime(value, format);

    public static Decimal ToDecimal(this string value)
        => ConvertHelper.ToDecimal(value);

    public static Decimal ToDecimal(this string value, bool reviseCulture)
        => ConvertHelper.ToDecimal(value, reviseCulture);

    public static Decimal ToDecimal(this string value, CultureInfo cultureInfo)
        => ConvertHelper.ToDecimal(value, cultureInfo);
}