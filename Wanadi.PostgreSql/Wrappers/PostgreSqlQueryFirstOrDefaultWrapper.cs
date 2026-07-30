using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static TType? QueryFirstOrDefault<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
        => QueryFirstOrDefaultAsync<TType>(connectionString, commandQuery, parameters).GetAwaiter().GetResult();

    public static TType? QueryFirstOrDefault<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
        => QueryFirstOrDefaultAsync<TType>(connection, commandQuery, parameters).GetAwaiter().GetResult();

    public static async Task<TType?> QueryFirstOrDefaultAsync<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            return await QueryFirstOrDefaultAsync<TType>(connection, commandQuery, parameters, cancellationToken);
        }
    }

    public static async Task<TType?> QueryFirstOrDefaultAsync<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where TType : class
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using (var command = new NpgsqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (parameters is not null && parameters.Count > 0)
            {
                parameters.ForEach(t => command.Parameters.Add(t));
            }

            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                if (!reader.HasRows)
                    return null;

                var resultFields = await GetResultFieldsAsync<TType>(reader, cancellationToken);
                if (resultFields.Count == 0)
                    return null;

                var response = new List<TType>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    return ConvertDataReaderToClass<TType>(reader, resultFields);
                }
            }
        }

        return null;
    }
}
