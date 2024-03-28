using System.Reflection;
using Npgsql.Schema;
using NpgsqlTypes;
using Wanadi.Common.Contracts.PropertyMappers;

namespace Wanadi.PostgreSql.Contracts;

public class PostgreSqlPropertyDataType : PropertyDataType
{
    public PostgreSqlPropertyDataType(PropertyInfo propertyInfo) : base(propertyInfo)
    {
    }

    public NpgsqlDbType PostgreSqlType { get; set; }
    public int ColumnIndex { get; set; }
    public Type DataType { get; set; }

    public PostgreSqlPropertyDataType SetDbInfo(NpgsqlDbColumn dbColumn)
    {
        if (dbColumn.AllowDBNull.HasValue)
            AllowNull = dbColumn.AllowDBNull.Value;

        PostgreSqlType = dbColumn.NpgsqlDbType.Value;

        ColumnIndex = dbColumn.ColumnOrdinal.Value;
        DataType = dbColumn.DataType;

        return this;
    }
}