namespace Wanadi.Common.Extensions;

public static class CompressionExtensions
{
    public static string ToBase64(this string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}