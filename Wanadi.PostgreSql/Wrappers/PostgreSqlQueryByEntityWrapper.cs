using Npgsql;
using Wanadi.Common.Extensions;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static List<TType> SelectQueryByEntity<TType>(string connectionString, int? limit = null) where TType : class
        => SelectQueryByEntityAsync<TType>(connectionString, limit).GetAwaiter().GetResult();

    public static List<TType> SelectQueryByEntity<TType>(NpgsqlConnection connection, int? limit = null) where TType : class
        => SelectQueryByEntityAsync<TType>(connection, limit).GetAwaiter().GetResult();

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(string connectionString, int? limit = null) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryByEntityAsync<TType>(connection, limit);
        }
    }

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(NpgsqlConnection connection, int? limit = null) where TType : class
    {
        var commandQuery = $"SELECT * FROM {typeof(TType).GetTableName()}";

        if (limit.GetValueOrDefault(0) > 0)
            commandQuery += $" LIMIT {limit.GetValueOrDefault(0)}";

        commandQuery += ";";

        return await SelectQueryAsync<TType>(connection, commandQuery);
    }
}