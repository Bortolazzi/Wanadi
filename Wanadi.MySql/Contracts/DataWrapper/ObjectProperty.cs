using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Wanadi.Common.Attributes;
using Wanadi.Common.Enums;
using Wanadi.Common.Extensions;

namespace Wanadi.MySql.Contracts.DataWrapper;

public class ObjectProperty
{
    public ObjectProperty(PropertyInfo propertyInfo, EnumConditions enumOption, GuidConditions guidOption)
    {
        Name = propertyInfo.Name;
        PropertyType = propertyInfo.PropertyType;

        if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            PropertyType = Nullable.GetUnderlyingType(PropertyType);
            AllowNull = true;
        }

        if (PropertyType.FullName.StartsWith("System.Collection") ||
            PropertyType.FullName.StartsWith("System.Object") ||
            !PropertyType.FullName.StartsWith("System"))
        {
            IgnoreOnInsert = true;
        }

        if (PropertyType == typeof(string))
            AllowNull = true;
        
        var databaseGenerated = propertyInfo.GetAttribute<DatabaseGeneratedAttribute>();
        if (databaseGenerated != null)
            IgnoreOnInsert = true;

        if (PropertyType.BaseType?.FullName == "System.Enum")
        {
            IsEnum = true;

            if (enumOption == EnumConditions.CastToInt || enumOption == EnumConditions.CastToString)
                IgnoreOnInsert = false;
        }

        if (PropertyType == typeof(Guid) && guidOption == GuidConditions.IgnoreOnInsert)
            IgnoreOnInsert = true;

        if (IgnoreOnInsert)
            return;

        var requiredAttribute = propertyInfo.GetAttribute<RequiredAttribute>();
        if (requiredAttribute != null)
            AllowNull = false;

        var columnAttribute = propertyInfo.GetAttribute<ColumnAttribute>();
        if (columnAttribute != null)
            ColumnName = columnAttribute.Name;

        var stringLengthAttribute = propertyInfo.GetAttribute<StringLengthAttribute>();
        if (stringLengthAttribute != null)
        {
            MaximumLength = stringLengthAttribute.MaximumLength;
            MinimumLength = stringLengthAttribute.MinimumLength;
        }

        var decimalPrecisionAttribute = propertyInfo.GetAttribute<DecimalPrecisionAttribute>();
        if (decimalPrecisionAttribute != null)
        {
            Precision = decimalPrecisionAttribute.Precision;
            Scale = decimalPrecisionAttribute.Scale;
        }

        var dataTypeAttribute = propertyInfo.GetAttribute<DataTypeAttribute>();
        if (dataTypeAttribute != null && dataTypeAttribute.DataType == DataType.Date)
            OnlyDate = true;

        CastTypeToMySql(enumOption, guidOption);
    }

    public string Name { get; set; }

    public string ColumnName
    {
        get { return _columnName ?? Name; }
        set { _columnName = value; }
    }
    private string? _columnName;

    public int? MinimumLength { get; set; }

    public int? MaximumLength { get; set; }

    public int? Precision { get; set; }

    public int? Scale { get; set; }

    public Type PropertyType { get; set; }

    public bool OnlyDate { get; set; } = false;

    public string MySqlType { get; set; }

    public bool IgnoreOnInsert { get; set; }

    public bool AllowNull { get; set; }

    public bool IsEnum { get; set; }

    private void CastTypeToMySql(EnumConditions enumOption, GuidConditions guidOption)
    {
        if (PropertyType.BaseType?.FullName == "System.Enum")
        {
            if (enumOption == EnumConditions.CastToInt)
                MySqlType = "int";
            else if (enumOption == EnumConditions.CastToString)
                MySqlType = "varchar";

            return;
        }

        if (PropertyType == typeof(Guid) && guidOption == GuidConditions.CastToString)
        {
            MySqlType = "varchar";
            return;
        }

        if (PropertyType == typeof(DateTime))
        {
            MySqlType = "datetime";

            if (OnlyDate)
                MySqlType = "date";

            return;
        }

        if (PropertyType == typeof(bool))
        {
            MySqlType = "bit";
            return;
        }

        if (PropertyType == typeof(decimal) || PropertyType == typeof(float) || PropertyType == typeof(double))
        {
            MySqlType = "decimal";
            return;
        }

        if (PropertyType == typeof(short))
        {
            MySqlType = "smallint";
            return;
        }

        if (PropertyType == typeof(int))
        {
            MySqlType = "int";
            return;
        }

        if (PropertyType == typeof(long))
        {
            MySqlType = "bigint";
            return;
        }

        if (PropertyType == typeof(char))
        {
            MySqlType = "char";
            return;
        }

        if (PropertyType == typeof(string))
        {
            MySqlType = "varchar";
            return;
        }

        throw new Exception($"Property type is not mapped int DataWrapper. Property Type: {PropertyType.Name}");
    }
}