using System.Reflection;
using MySql.Data.MySqlClient;

namespace Wanadi.MySql.Contracts;

public class ColumnsResultDataType
{
    public ColumnsResultDataType(MySqlDataReader reader, int index)
    {
        ColumnIndex = index;
        ColumnName = reader.GetName(index);
        MySqlDataType = reader.GetDataTypeName(index).ToUpper();

        DataType = MySqlDataType switch
        {
            "INT" => typeof(Int32),
            "SMALLINT" => typeof(Int16),
            "MEDIUMINT" => typeof(Int32),
            "BIGINT" => typeof(Int64),

            "TINYINT"=> typeof(byte),

            "VARCHAR" => typeof(string),

            "DECIMAL" => typeof(decimal),
            "DOUBLE" => typeof(double),
            "FLOAT" => typeof(float),

            "DATE" => typeof(DateTime),
            "DATETIME" => typeof(DateTime),
            "TIMESTAMP"=>  typeof(DateTime),

            "TIME" => typeof(TimeSpan),
            "BIT" => typeof(bool),
            

            _ => throw new Exception($"MySql DataType not mapped: {MySqlDataType}")
        };
    }

    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; }
    public string MySqlDataType { get; set; }
    public Type? DataType { get; set; }

    public PropertyInfo Property { get; private set; }

    public ColumnsResultDataType SetPropertyInfo(PropertyInfo property)
    {
        Property = property;
        return this;
    }
}