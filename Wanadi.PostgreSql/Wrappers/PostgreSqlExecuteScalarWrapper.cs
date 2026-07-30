using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static object? ExecuteScalar(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteScalarAsync(connectionString, commandExecute, parameters).GetAwaiter().GetResult();

    public static object? ExecuteScalar(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteScalarAsync(connection, commandExecute, parameters).GetAwaiter().GetResult();

    public static async Task<object?> ExecuteScalarAsync(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            return await ExecuteScalarAsync(connection, commandExecute, parameters, cancellationToken);
        }
    }

    public static async Task<object?> ExecuteScalarAsync(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using (var command = new NpgsqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (parameters is not null && parameters.Count > 0)
            {
                parameters.ForEach(t => command.Parameters.Add(t));
            }

            return await command.ExecuteScalarAsync(cancellationToken);
        }
    }
}
