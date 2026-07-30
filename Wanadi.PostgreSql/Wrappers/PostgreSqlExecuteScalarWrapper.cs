using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static object? ExecuteScalar(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteScalarAsync(connectionString, commandExecute, parameters).GetAwaiter().GetResult();

    public static object? ExecuteScalar(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteScalarAsync(connection, commandExecute, parameters).GetAwaiter().GetResult();

    public static async Task<object?> ExecuteScalarAsync(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return ExecuteScalarAsync(connection, commandExecute, parameters);
        }
    }

    public static async Task<object?> ExecuteScalarAsync(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new NpgsqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (parameters is not null && parameters.Count > 0)
            {
                parameters.ForEach(t => command.Parameters.Add(t));
                await command.PrepareAsync();
            }

            return await command.ExecuteScalarAsync();
        }
    }
}