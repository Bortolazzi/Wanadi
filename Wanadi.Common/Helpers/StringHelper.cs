namespace Wanadi.Common.Helpers;

public static class StringHelper
{
    public static string? NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }
}