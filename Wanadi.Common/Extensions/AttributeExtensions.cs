using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Wanadi.Common.Extensions;

public static class AttributeExtensions
{
    public static T? GetAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
    {
        var attributeType = typeof(T);

        var customAttribute = propertyInfo.GetCustomAttributes(attributeType, false);
        if (customAttribute == null)
            return null;

        var responseAttribute = customAttribute.FirstOrDefault();
        if (responseAttribute == null)
            return null;

        return (T)responseAttribute;
    }

    public static T? GetAttribute<T>(this Type type) where T : Attribute
    {
        var attributeType = typeof(T);

        var customAttribute = type.GetCustomAttributes(attributeType, false);
        if (customAttribute == null)
            return null;

        var responseAttribute = customAttribute.FirstOrDefault();
        if (responseAttribute == null)
            return null;

        return (T)responseAttribute;
    }

    public static string GetTableName(this Type type)
    {
        var tableNameAttribute = type.GetAttribute<TableAttribute>();
        return tableNameAttribute?.Name ?? type.Name;
    }
}