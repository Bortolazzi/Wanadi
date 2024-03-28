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
        OriginalPropertyInfo = propertyInfo;

        Name = propertyInfo.Name;
        PropertyType = propertyInfo.PropertyType;

        if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            PropertyType = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
            AllowNull = true;
        }

        var fullName = PropertyType.FullName ?? string.Empty;
        if (fullName.StartsWith("System.Collection") ||
            fullName.StartsWith("System.Object") ||
            !fullName.StartsWith("System"))
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
        if (columnAttribute != null && !string.IsNullOrEmpty(columnAttribute.Name))
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

    public PropertyInfo OriginalPropertyInfo { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Nome da propriedade. Identica ao que está no código.
    ///     </para>
    ///     <para>
    ///         en-US: Property name. Identical to what is in the code.
    ///     </para>
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Valor encontrado no ColumnAttribute. Caso não possua será retornado a propriedade Name. 
    ///     </para>
    ///     <para>
    ///         en-US: Value found in ColumnAttribute. If not, the Name property will be returned.
    ///     </para>
    /// </summary>
    public string ColumnName
    {
        get { return _columnName ?? Name; }
        set { _columnName = value; }
    }
    private string? _columnName;

    /// <summary>
    ///     <para>
    ///         pt-BR: Valor encontrado no MinimumLength do StringLengthAttribute ou nulo para as propriedades que não possuem.
    ///     </para>
    ///     <para>
    ///         en-US: Value found in the MinimumLength of the StringLengthAttribute or null for properties that do not have it.
    ///     </para>
    /// </summary>
    public int? MinimumLength { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Valor encontrado no MaximumLength do StringLengthAttribute ou nulo para as propriedades que não possuem.
    ///     </para>
    ///     <para>
    ///         en-US: Value found in the MaximumLength of the StringLengthAttribute or null for properties that do not have it.
    ///     </para>
    /// </summary>
    public int? MaximumLength { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Valor encontrado no Precision do DecimalPrecisionAttribute ou nulo para as propriedades que não possuem.
    ///     </para>
    ///     <para>
    ///         en-US: Value found in the Precision of the DecimalPrecisionAttribute or null for properties that do not have it.
    ///     </para>
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Valor encontrado no Scale do DecimalPrecisionAttribute ou nulo para as propriedades que não possuem.
    ///     </para>
    ///     <para>
    ///         en-US: Value found in the Scale of the DecimalPrecisionAttribute or null for properties that do not have it.
    ///     </para>
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Tipo da propriedade encontrada no objeto.
    ///     </para>
    ///     <para>
    ///         en-US: Type of property found on the object.
    ///     </para>
    /// </summary>
    public Type PropertyType { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de apenas Data para as propriedades do tipo DateTime que possuem o atributo DataTypeAttribute(DataType.Date)
    ///     </para>
    ///     <para>
    ///         en-US: Date-only indicator for DateTime properties that have the DataTypeAttribute(DataType.Date)
    ///     </para>
    /// </summary>
    public bool OnlyDate { get; set; } = false;

    /// <summary>
    ///     <para>
    ///         pt-BR: Tipo de dado correspondente para Mysql.
    ///     </para>
    ///     <para>
    ///         en-US: Corresponding data type for Mysql.
    ///     </para>
    /// </summary>
    public string MySqlType { get; set; } = string.Empty;

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de remoção da propriedade para criação do comando de inserção.
    ///     </para>
    ///     <para>
    ///         en-US: Property ignore indicator for creating the insert command.
    ///     </para>
    ///     <para>
    ///         Exemplo de casos de remoção/Example of removal cases:
    ///     </para>
    ///
    ///     <para>
    ///         List, IList, Dictionary, Object, Classes, DatabaseGeneratedAttribute, Enum (DataWrapper.EnumOption = IgnoreOnInsert), Guid (DataWrapper.GuidOption = IgnoreOnInsert)
    ///     </para>
    /// </summary>
    public bool IgnoreOnInsert { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de propriedade que pode possuir valor NULL.
    ///     </para>
    ///     <para>
    ///         en-US: Property indicator that can have a NULL value.
    ///     </para>
    /// </summary>
    public bool AllowNull { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de propriedade do tipo Enum.
    ///     </para>
    ///     <para>
    ///         en-US: Enum type property indicator.
    ///     </para>
    /// </summary>
    public bool IsEnum { get; set; }

    private void CastTypeToMySql(EnumConditions enumOption, GuidConditions guidOption)
    {
        if (IsEnum)
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

        if (PropertyType == typeof(decimal))
        {
            MySqlType = "decimal";
            return;
        }

        if (PropertyType == typeof(float))
        {
            MySqlType = "float";
            return;
        }

        if (PropertyType == typeof(double))
        {
            MySqlType = "double";
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

        if (PropertyType == typeof(TimeSpan))
        {
            MySqlType = "time";
            return;
        }

        if (PropertyType == typeof(byte))
        {
            MySqlType = "byte";
            return;
        }

        if (PropertyType == typeof(char[]))
        {
            MySqlType = "char[]";
            return;
        }

        throw new Exception($"Property type is not mapped int DataWrapper. Property Type: {PropertyType.Name}");
    }
}