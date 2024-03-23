using System.Reflection;
using ClickHouse.Ado;

namespace Wanadi.Clickhouse.Contracts;

public class ClickHouseColumnsResultDataType
{
    public ClickHouseColumnsResultDataType(ClickHouseDataReader reader, int index)
    {
        ColumnIndex = index;
        ColumnName = reader.GetName(index);
        ClickHouseDataType = reader.GetDataTypeName(index);

        DataType = ClickHouseDataType switch
        {
            "UUID" => typeof(Guid),
            "Nullable(UUID)" => typeof(Guid),

            "Date" => typeof(DateTime),
            "Nullable(Date)" => typeof(DateTime),

            "DateTime" => typeof(DateTime),
            "Nullable(DateTime)" => typeof(DateTime),

            "UInt8" => typeof(byte),
            "Nullable(UInt8)" => typeof(byte),

            "UInt16" => typeof(UInt16),
            "Nullable(UInt16)" => typeof(UInt16),

            "UInt32" => typeof(UInt32),
            "Nullable(UInt32)" => typeof(UInt32),

            "UInt64" => typeof(UInt64),
            "Nullable(UInt64)" => typeof(UInt64),

            "Int8" => typeof(byte),
            "Nullable(Int8)" => typeof(byte),

            "Int16" => typeof(Int16),
            "Nullable(Int16)" => typeof(Int16),

            "Int32" => typeof(Int32),
            "Nullable(Int32)" => typeof(Int32),

            "Int64" => typeof(Int64),
            "Nullable(Int64)" => typeof(Int64),

            "Float32" => typeof(float),
            "Nullable(Float32)" => typeof(float),

            "Float64" => typeof(double),
            "Nullable(Float64)" => typeof(double),

            "Bool" => typeof(bool),
            "Nullable(Bool)" => typeof(bool),

            "String" => typeof(string),
            "Nullable(String)" => typeof(string),

            _ => null
        };

        if (DataType is null)
        {
            if (ClickHouseDataType.Contains("Decimal"))
                DataType = typeof(decimal);

            if (ClickHouseDataType.Contains("Float"))
                DataType = typeof(float);

            if (ClickHouseDataType.Contains("Double"))
                DataType = typeof(double);

            if (ClickHouseDataType.Contains("FixedString"))
                DataType = typeof(string);

            if (DataType is null)
                throw new Exception($"ClickHouse DataType not mapped: {ClickHouseDataType}");
        }

        AllowNull = ClickHouseDataType.Contains("Nullable");
    }

    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; }
    public string ClickHouseDataType { get; set; }
    public bool AllowNull { get; set; } = false;
    public Type? DataType { get; set; }

    public PropertyInfo Property { get; private set; }

    public ClickHouseColumnsResultDataType SetPropertyInfo(PropertyInfo property)
    {
        Property = property;
        return this;
    }
}