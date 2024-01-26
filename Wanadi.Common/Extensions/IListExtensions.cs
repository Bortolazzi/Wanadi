using System.Collections;

namespace Wanadi.Common.Extensions;

public static class IListExtensions
{
    public static string? GetTableName(this IList list)
    {
        if (list == null)
            return null;

        var objectType = list.GetType().GetGenericArguments()[0];

        return objectType.GetTableName();
    }
}