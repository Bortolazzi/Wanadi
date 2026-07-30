using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static List<TType> Query<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        return QueryAsync<TType>(connectionString, commandQuery, parameters)
            .GetAwaiter()
            .GetResult();
    }

    public static List<TType> Query<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        return QueryAsync<TType>(connection, commandQuery, parameters)
            .GetAwaiter()
            .GetResult();
    }

    public static async Task<List<TType>> QueryAsync<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            return await QueryAsync<TType>(connection, commandQuery, parameters, cancellationToken);
        }
    }

    public static async Task<List<TType>> QueryAsync<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where TType : class
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using (var command = new NpgsqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (parameters is not null && parameters.Count > 0)
            {
                parameters.ForEach(t => command.Parameters.Add(t));
                await command.PrepareAsync(cancellationToken);
            }

            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                if (!reader.HasRows)
                    return new List<TType>();

                var resultFields = await GetResultFieldsAsync<TType>(reader, cancellationToken);

                if (resultFields.Any() is false)
                    return null;

                var response = new List<TType>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    response.Add(ConvertDataReaderToClass<TType>(reader, resultFields));
                }

                return response;
            }
        }
    }
}
