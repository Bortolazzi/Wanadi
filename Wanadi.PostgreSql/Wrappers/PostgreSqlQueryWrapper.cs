using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static List<TType> SelectQuery<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        return SelectQueryAsync<TType>(connectionString, commandQuery, parameters)
            .GetAwaiter()
            .GetResult();
    }

    public static List<TType> SelectQuery<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        return SelectQueryAsync<TType>(connection, commandQuery, parameters)
            .GetAwaiter()
            .GetResult();
    }

    public static async Task<List<TType>> SelectQueryAsync<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryAsync<TType>(connection, commandQuery, parameters);
        }
    }

    public static async Task<List<TType>> SelectQueryAsync<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new NpgsqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (parameters is not null && parameters.Count > 0)
            {
                parameters.ForEach(t => command.Parameters.Add(t));
                await command.PrepareAsync();
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                    return new List<TType>();

                var resultFields = await GetResultFieldsAsync<TType>(reader);

                if (resultFields.Any() is false)
                    return null;

                var response = new List<TType>();

                while (await reader.ReadAsync())
                {
                    response.Add(ConvertDataReaderToClass<TType>(reader, resultFields));
                }

                return response;
            }
        }
    }
}