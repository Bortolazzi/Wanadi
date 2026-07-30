using Npgsql;
using Wanadi.Common.Extensions;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static List<TType> QueryByEntity<TType>(string connectionString, int? limit = null) where TType : class
        => QueryByEntityAsync<TType>(connectionString, limit).GetAwaiter().GetResult();

    public static List<TType> QueryByEntity<TType>(NpgsqlConnection connection, int? limit = null) where TType : class
        => QueryByEntityAsync<TType>(connection, limit).GetAwaiter().GetResult();

    public static async Task<List<TType>> QueryByEntityAsync<TType>(string connectionString, int? limit = null, CancellationToken cancellationToken = default) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            return await QueryByEntityAsync<TType>(connection, limit, cancellationToken);
        }
    }

    public static async Task<List<TType>> QueryByEntityAsync<TType>(NpgsqlConnection connection, int? limit = null, CancellationToken cancellationToken = default) where TType : class
    {
        var commandQuery = $"SELECT * FROM {typeof(TType).GetTableName()}";

        if (limit.GetValueOrDefault(0) > 0)
            commandQuery += $" LIMIT {limit.GetValueOrDefault(0)}";

        commandQuery += ";";

        return await QueryAsync<TType>(connection, commandQuery, cancellationToken: cancellationToken);
    }
}
