using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Wanadi.Common.Extensions;

namespace Wanadi.Clickhouse.Contracts;

public class PropertyDataWrapper
{
    public PropertyDataWrapper(PropertyInfo property)
    {
        OriginalPropertyInfo = property;
        Name = property.Name;

        var columnAttribute = property.GetAttribute<ColumnAttribute>();
        if (columnAttribute != null)
            ColumnName = columnAttribute.Name;

        var databaseGeneratedAttribute = property.GetAttribute<DatabaseGeneratedAttribute>();
        if (databaseGeneratedAttribute != null)
            IgnoreOnInsert = true;

        PropertyType = property.PropertyType;

        if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            PropertyType = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
            AllowNull = true;
        }
    }

    public PropertyInfo OriginalPropertyInfo { get; set; }
    public string Name { get; set; }
    public string ColumnName
    {
        get { return _columnName ?? Name; }
        set { _columnName = value; }
    }
    private string? _columnName;

    public Type PropertyType { get; set; }
    public bool AllowNull { get; set; } = false;
    public bool IgnoreOnInsert { get; set; } = false;
}