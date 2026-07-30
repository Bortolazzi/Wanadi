using System.Reflection;
using System.Data.Common;
using Npgsql.Schema;
using NpgsqlTypes;
using Wanadi.Common.Contracts.PropertyMappers;

namespace Wanadi.PostgreSql.Contracts;

public record PostgreSqlPropertyDataType : PropertyDataType
{
    public PostgreSqlPropertyDataType(PropertyInfo propertyInfo) : base(propertyInfo)
    {
    }

    public NpgsqlDbType? PostgreSqlType { get; set; }
    public int ColumnIndex { get; set; }
    public Type? DataType { get; set; }
    public string? DataTypeName { get; set; }

    public PostgreSqlPropertyDataType SetDbInfo(DbColumn dbColumn)
    {
        if (dbColumn.AllowDBNull.HasValue)
            AllowNull = dbColumn.AllowDBNull.Value;

        if (dbColumn is NpgsqlDbColumn npgsqlColumn)
            PostgreSqlType = npgsqlColumn.NpgsqlDbType;

        ColumnIndex = dbColumn.ColumnOrdinal.GetValueOrDefault();
        DataType = dbColumn.DataType;
        DataTypeName = dbColumn.DataTypeName;

        return this;
    }
}
