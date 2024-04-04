using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Wanadi.Common.Contracts.PropertyMappers;

public class PropertyDataType
{
    public PropertyDataType(PropertyInfo property)
    {
        PropertyInfo = property;
        Name = property.Name;

        var columnAttribute = property.GetAttribute<ColumnAttribute>();
        if (columnAttribute != null)
            ColumnName = columnAttribute.Name;

        var databaseGeneratedAttribute = property.GetAttribute<DatabaseGeneratedAttribute>();
        if (databaseGeneratedAttribute != null)
            IgnoreOnInsert = true;

        var keyAttribute = property.GetAttribute<KeyAttribute>();
        if (keyAttribute != null)
            HasKeyAttribute = true;

        PropertyType = property.PropertyType;

        if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            PropertyType = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
            AllowNull = true;
        }

        if (PropertyType == typeof(string))
        {
            var nullableAttribute = property.GetAttribute<System.Runtime.CompilerServices.NullableAttribute>();
            if (nullableAttribute != null)
                AllowNull = true;
        }
    }

    public PropertyInfo PropertyInfo { get; set; }
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
    public bool HasKeyAttribute { get; set; } = false;
}