using System.Data;
using Npgsql;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
     public static DataTable Fill(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => FillAsync(connectionString, commandExecute, parameters).GetAwaiter().GetResult();

    public static DataTable Fill(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => FillAsync(connection, commandExecute, parameters).GetAwaiter().GetResult();

    public static async Task<DataTable> FillAsync(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await FillAsync(connection, commandExecute, parameters);
        }
    }

    public static async Task<DataTable> FillAsync(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new NpgsqlCommand(commandExecute, connection))
        using (var dataAdapter = new NpgsqlDataAdapter(command))
        {
            command.CommandType = CommandType.Text;
            if (parameters is not null && parameters.Count > 0)
            {
                parameters.ForEach(t => command.Parameters.Add(t));
                await command.PrepareAsync();
            }

            var response = new DataTable();
            dataAdapter.Fill(response);
            return response;
        }
    }
}