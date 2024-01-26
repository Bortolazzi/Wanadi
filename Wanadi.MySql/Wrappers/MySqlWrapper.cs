using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using MySql.Data.MySqlClient;
using Wanadi.Common.Extensions;

namespace Wanadi.MySql.Wrappers;

public static class MySqlWrapper
{
    public static string ConnectionString = string.Empty;

    public static string BuildConnectionString(string server, string database, string user, string password, uint commandTimeout = 180)
           => new MySqlConnectionStringBuilder
           {
               AllowUserVariables = true,
               Server = server,
               Database = database,
               UserID = user,
               Password = password,
               Port = 3306,
               SslMode = MySqlSslMode.Disabled,
               PersistSecurityInfo = true,
               ConvertZeroDateTime = true,
               AllowZeroDateTime = true,
               IncludeSecurityAsserts = true,
               MaximumPoolSize = 5000,
               DefaultCommandTimeout = commandTimeout,
               ConnectionTimeout = commandTimeout
           }.ConnectionString;

    private static List<string> GetResultFields(MySqlDataReader dataReader)
        => Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName).ToList();

    public static T ConvertDataReaderToClass<T>(MySqlDataReader reader, List<string> resultFields) where T : class
    {
        T response = Activator.CreateInstance<T>();
        var properties = response.GetType().GetProperties();

        foreach (var propertyInfo in properties)
        {
            var columnAttribute = propertyInfo.GetAttribute<ColumnAttribute>();
            var columnName = columnAttribute?.Name ?? propertyInfo.Name;

            if (!resultFields.Contains(columnName))
                continue;

            object value = reader[columnName];

            if (value == DBNull.Value || value == null)
                continue;

            var dataType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            if (dataType == typeof(Guid) && value.GetType() == typeof(string))
            {
                propertyInfo.SetValue(response, Guid.Parse(value.ToString()), null);
                continue;
            }

            if (dataType == typeof(DateTime) || dataType == typeof(Boolean))
            {
                propertyInfo.SetValue(response, Convert.ChangeType(value, dataType), null);
                continue;
            }

            propertyInfo.SetValue(response, value, null);
        }

        return response;
    }

    public static List<TType> SelectQuery<TType>(string commandQuery) where TType : class
        => SelectQuery<TType>(ConnectionString, commandQuery);

    public static List<TType> SelectQuery<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;
            command.Connection.Open();

            using (MySqlDataReader dataReader = command.ExecuteReader())
            {
                if (!dataReader.HasRows)
                    return new List<TType>();

                var resultFields = GetResultFields(dataReader);
                var response = new List<TType>();

                while (dataReader.Read())
                {
                    response.Add(ConvertDataReaderToClass<TType>(dataReader, resultFields));
                }

                return response;
            }
        }
    }

    public static List<TType> SelectQueryByEntity<TType>(int limit = -1) where TType : class
        => SelectQueryByEntity<TType>(ConnectionString, limit);

    public static List<TType> SelectQueryByEntity<TType>(string connectionString, int limit = -1) where TType : class
    {
        var commandQuery = $"SELECT * FROM `{typeof(TType).GetTableName()}`";

        if (limit > 0)
            commandQuery += $" LIMIT {limit}";

        commandQuery += ";";

        return SelectQuery<TType>(connectionString, commandQuery);
    }

    public static TType? SelectQueryFirstOrDefault<TType>(string commandQuery) where TType : class
        => SelectQueryFirstOrDefault<TType>(ConnectionString, commandQuery);

    public static TType? SelectQueryFirstOrDefault<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;
            command.Connection.Open();

            using (MySqlDataReader dataReader = command.ExecuteReader())
            {
                if (!dataReader.HasRows)
                    return null;

                var resultFields = GetResultFields(dataReader);

                while (dataReader.Read())
                {
                    return ConvertDataReaderToClass<TType>(dataReader, resultFields);
                }
            }
        }

        return null;
    }

    public static async Task<List<TType>> SelectQueryAsync<TType>(string commandQuery) where TType : class
        => await SelectQueryAsync<TType>(commandQuery);

    public static async Task<List<TType>> SelectQueryAsync<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            await command.Connection.OpenAsync();

            using (var dataReader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (!dataReader.HasRows)
                    return new List<TType>();

                var resultFields = GetResultFields(dataReader);
                var response = new List<TType>();

                while (await dataReader.ReadAsync())
                {
                    response.Add(MySqlWrapper.ConvertDataReaderToClass<TType>(dataReader, resultFields));
                }

                return response;
            }
        }
    }

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(int limit = -1) where TType : class
        => await SelectQueryByEntityAsync<TType>(ConnectionString, limit);

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(string connectionString, int limit = -1) where TType : class
    {
        var commandQuery = $"SELECT * FROM `{typeof(TType).GetTableName()}`";

        if (limit > 0)
            commandQuery += $" LIMIT {limit}";

        commandQuery += ";";

        return await SelectQueryAsync<TType>(connectionString, commandQuery);
    }

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(string commandQuery) where TType : class
        => await SelectQueryFirstOrDefaultAsync<TType>(ConnectionString, commandQuery);

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            await command.Connection.OpenAsync();

            using (var dataReader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (!dataReader.HasRows)
                    return null;

                var resultFields = GetResultFields(dataReader);
                var response = new List<TType>();

                while (await dataReader.ReadAsync())
                {
                    return MySqlWrapper.ConvertDataReaderToClass<TType>(dataReader, resultFields);
                }
            }
        }

        return null;
    }

    public static int ExecuteNonQuery(string commandExecute)
        => ExecuteNonQuery(ConnectionString, commandExecute);

    public static int ExecuteNonQuery(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;
            command.Connection.Open();
            return command.ExecuteNonQuery();
        }
    }

    public static async Task<int> ExecuteNonQueryAsync(string commandExecute)
        => await ExecuteNonQueryAsync(ConnectionString, commandExecute);

    public static async Task<int> ExecuteNonQueryAsync(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            await command.Connection.OpenAsync();

            return await command.ExecuteNonQueryAsync();
        }
    }

    public static object ExecuteScalar(string commandExecute)
        => ExecuteScalar(ConnectionString, commandExecute);

    public static object ExecuteScalar(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            command.Connection.Open();
            return command.ExecuteScalar();
        }
    }

    public static async Task<object?> ExecuteScalarAsync(string commandExecute)
        => await ExecuteScalarAsync(ConnectionString, commandExecute);

    public static async Task<object?> ExecuteScalarAsync(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            await command.Connection.OpenAsync();
            return await command.ExecuteScalarAsync();
        }
    }

    public static DataTable Fill(string commandExecute)
        => Fill(ConnectionString, commandExecute);

    public static DataTable Fill(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            connection.Open();

            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command))
            {
                var response = new DataTable();
                dataAdapter.Fill(response);
                return response;
            }
        }
    }

    public static async Task<DataTable> FillAsync(string commandExecute)
        => await FillAsync(ConnectionString, commandExecute);

    public static async Task<DataTable> FillAsync(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            await connection.OpenAsync();

            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command))
            {
                var response = new DataTable();
                await dataAdapter.FillAsync(response);
                return response;
            }
        }
    }
}