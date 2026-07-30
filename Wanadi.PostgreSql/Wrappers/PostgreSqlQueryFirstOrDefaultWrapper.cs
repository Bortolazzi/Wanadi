using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static TType? SelectQueryFirstOrDefault<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
        => SelectQueryFirstOrDefaultAsync<TType>(connectionString, commandQuery, parameters).GetAwaiter().GetResult();

    public static TType? SelectQueryFirstOrDefault<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
        => SelectQueryFirstOrDefaultAsync<TType>(connection, commandQuery, parameters).GetAwaiter().GetResult();

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryFirstOrDefaultAsync<TType>(connection, commandQuery, parameters);
        }
    }

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
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
                    return null;

                var resultFields = await GetResultFieldsAsync<TType>(reader);
                if (resultFields.Count == 0)
                    return null;

                var response = new List<TType>();

                while (await reader.ReadAsync())
                {
                    return ConvertDataReaderToClass<TType>(reader, resultFields);
                }
            }
        }

        return null;
    }
}