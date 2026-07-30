using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static int ExecuteNonQuery(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
       => ExecuteNonQueryAsync(connectionString, commandExecute, parameters).GetAwaiter().GetResult();

    public static int ExecuteNonQuery(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteNonQueryAsync(connection, commandExecute, parameters).GetAwaiter().GetResult();

    public static async Task<int> ExecuteNonQueryAsync(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            return await ExecuteNonQueryAsync(connection, commandExecute, parameters, cancellationToken);
        }
    }

    public static async Task<int> ExecuteNonQueryAsync(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
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

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
