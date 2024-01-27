using MySql.Data.MySqlClient;
using Wanadi.Common.Enums;
using Wanadi.Common.Extensions;

namespace Wanadi.MySql.Contracts.DataWrapper;

public class ValueProperty
{
    public ValueProperty(Type objectType, object item, ObjectProperty objectProperty, EnumConditions enumOption, GuidConditions guidOption)
    {
        ColumnName = objectProperty.ColumnName;

        var value = objectType.GetProperty(objectProperty.Name).GetValue(item, null);
        if (value == null)
        {
            if (!objectProperty.AllowNull)
                throw new Exception($"Property {objectProperty.Name} cannot be null");

            Value = "NULL";
            return;
        }

        if (objectProperty.IsEnum)
        {
            if (enumOption == EnumConditions.IgnoreOnInsert)
                throw new Exception($"Enum is not allowed to insert. Change DataWrapper.EnumOption and try again.");

            if (enumOption == EnumConditions.CastToInt)
                Value = ((int)value).ToString();

            if (enumOption == EnumConditions.CastToString)
                Value = $"'{MySqlHelper.EscapeString(((Enum)value).Description())}'";

            return;
        }

        if (objectProperty.PropertyType == typeof(short))
        {
            Value = value.ToString();
            return;
        }

        if (objectProperty.PropertyType == typeof(int))
        {
            Value = value.ToString();
            return;
        }

        if (objectProperty.PropertyType == typeof(long))
        {
            Value = value.ToString();
            return;
        }

        if (objectProperty.PropertyType == typeof(bool))
        {
            Value = Convert.ToBoolean(value) ? "1" : "0";
            return;
        }

        if (objectProperty.PropertyType == typeof(DateTime))
        {
            Value = $"{Convert.ToDateTime(value):yyyy-MM-dd HH:mm:ss}";
            Value = Value.Replace("00:00:00", string.Empty).Trim();
            if (objectProperty.OnlyDate)
                Value = Value.Split(' ')[0];

            Value = $"'{Value}'";
            return;
        }

        if (objectProperty.PropertyType == typeof(Guid))
        {
            if (guidOption == GuidConditions.IgnoreOnInsert)
                throw new Exception("Guid is not allowed to insert. Change DataWrapper.GuidOption and try again.");

            Value = $"'{value}'";
        }

        if (objectProperty.PropertyType == typeof(string))
        {
            if (objectProperty.MaximumLength.HasValue && value.ToString().Length > objectProperty.MaximumLength.Value)
                throw new Exception($"Value {value} too long to column {ColumnName}.");

            if (objectProperty.MinimumLength.HasValue && value.ToString().Length < objectProperty.MinimumLength.Value)
                throw new Exception($"Value {value} too small to column {ColumnName}.");

            Value = $"'{MySqlHelper.EscapeString(value.ToString())}'";
            return;
        }

        if (objectProperty.PropertyType == typeof(char))
        {
            Value = $"'{MySqlHelper.EscapeString(value.ToString())}'";
            return;
        }

        if (objectProperty.PropertyType == typeof(decimal))
        {
            if (objectProperty.Scale.HasValue)
            {
                Value = $"{decimal.Round(Convert.ToDecimal(value), objectProperty.Scale.Value, MidpointRounding.AwayFromZero).ToString().Replace(",", ".")}";
                return;
            }

            Value = $"{value.ToString().Replace(",", ".")}";
            return;
        }

        if (objectProperty.PropertyType == typeof(float))
        {
            Value = $"{value.ToString().Replace(",", ".")}'";
            return;
        }

        if (objectProperty.PropertyType == typeof(double))
        {
            Value = $"{value.ToString().Replace(",", ".")}'";
            return;
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Nome da coluna para inserção no banco de dados.
    ///     </para>
    ///     <para>
    ///         en-US: Column name to insert in database.
    ///     </para>
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Valor pronto para inserção no banco de dados. Quando necessário já irá vir com aspas simples.
    ///     </para>
    ///     <para>
    ///         en-US: Value ready for insert into the database. When necessary, it will come with single quotes.
    ///     </para>
    /// </summary>
    public string Value { get; set; }
}