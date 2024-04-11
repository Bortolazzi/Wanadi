using System.Data;
using Npgsql;
using Npgsql.Schema;
using Wanadi.Common.Extensions;
using Wanadi.PostgreSql.Contracts;

namespace Wanadi.PostgreSql.Wrappers;

public static class PostgreSqlWrapper
{
    #region [ConnectionString]

    public static string BuildConnectionString(PostgreSqlConnectionSettings settings, string database)
        => BuildConnectionString(settings.Host, settings.Username, settings.Password, database, settings.Port, settings.Timeout, settings.CommandTimeout, settings.MaxPoolSize, settings.CancellationTimeout);

    public static string BuildConnectionString(
        string host,
        string username,
        string password,
        string? database,
        int? port = 5432,
        int? timeout = 15,
        int? commandTimeout = 30,
        int? maxPoolSize = 5000,
        int? cancellationTimeout = 2000)
    {
        var builder = new NpgsqlConnectionStringBuilder()
        {
            Host = host,
            Username = username,
            Password = password,
            Port = port ?? 5432,
            SslMode = SslMode.Prefer,
            Timeout = timeout ?? 15,
            CommandTimeout = commandTimeout ?? 30,
            Database = database,
            PersistSecurityInfo = true,

            MaxPoolSize = maxPoolSize ?? 5000,
            CancellationTimeout = cancellationTimeout ?? 2000
        };

        return builder.ConnectionString;
    }

    #endregion [ConnectionString]

    #region [GetConnectionAsync]

    public static NpgsqlConnection GetConnection(string connectionString)
        => GetConnectionAsync(connectionString).GetAwaiter().GetResult();

    public static async Task<NpgsqlConnection> GetConnectionAsync(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    #endregion [GetConnectionAsync]

    #region [SelectQueryAsync]

    public static List<TType> SelectQuery<TType>(string connectionString, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
        => SelectQueryAsync<TType>(connectionString, commandQuery, parameters).GetAwaiter().GetResult();

    public static List<TType> SelectQuery<TType>(NpgsqlConnection connection, string commandQuery, List<NpgsqlParameter>? parameters = null) where TType : class
        => SelectQueryAsync<TType>(connection, commandQuery, parameters).GetAwaiter().GetResult();

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

                if (resultFields.Count == 0)
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

    #endregion [SelectQueryAsync]

    #region [SelectQueryByEntity]

    public static List<TType> SelectQueryByEntity<TType>(string connectionString, int? limit = null) where TType : class
        => SelectQueryByEntityAsync<TType>(connectionString, limit).GetAwaiter().GetResult();

    public static List<TType> SelectQueryByEntity<TType>(NpgsqlConnection connection, int? limit = null) where TType : class
        => SelectQueryByEntityAsync<TType>(connection, limit).GetAwaiter().GetResult();

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(string connectionString, int? limit = null) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryByEntityAsync<TType>(connection, limit);
        }
    }

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(NpgsqlConnection connection, int? limit = null) where TType : class
    {
        var commandQuery = $"SELECT * FROM {typeof(TType).GetTableName()}";

        if (limit.GetValueOrDefault(0) > 0)
            commandQuery += $" LIMIT {limit.GetValueOrDefault(0)}";

        commandQuery += ";";

        return await SelectQueryAsync<TType>(connection, commandQuery);
    }

    #endregion [SelectQueryByEntity]

    #region [SelectQueryFirstOrDefaultAsync]

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

    #endregion [SelectQueryFirstOrDefaultAsync]

    #region [ExecuteNonQuery]

    public static int ExecuteNonQuery(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteNonQueryAsync(connectionString, commandExecute, parameters).GetAwaiter().GetResult();

    public static int ExecuteNonQuery(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
        => ExecuteNonQueryAsync(connection, commandExecute, parameters).GetAwaiter().GetResult();

    public static async Task<int> ExecuteNonQueryAsync(string connectionString, string commandExecute, List<NpgsqlParameter>? parameters = null)
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await ExecuteNonQueryAsync(connection, commandExecute, parameters);
        }
    }

    public static async Task<int> ExecuteNonQueryAsync(NpgsqlConnection connection, string commandExecute, List<NpgsqlParameter>? parameters = null)
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

            return await command.ExecuteNonQueryAsync();
        }
    }

    #endregion [ExecuteNonQuery]

    #region [ExecuteScalar]

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

    #endregion [ExecuteScalar]

    #region [Fill]

    public static DataTable Fill(string connectionString, string commandExecute)
        => FillAsync(connectionString, commandExecute).GetAwaiter().GetResult();

    public static DataTable Fill(NpgsqlConnection connection, string commandExecute)
        => FillAsync(connection, commandExecute).GetAwaiter().GetResult();

    public static async Task<DataTable> FillAsync(string connectionString, string commandExecute)
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await FillAsync(connection, commandExecute);
        }
    }

    public static async Task<DataTable> FillAsync(NpgsqlConnection connection, string commandExecute)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new NpgsqlCommand(commandExecute, connection))
        using (var dataAdapter = new NpgsqlDataAdapter(command))
        {
            var response = new DataTable();
            dataAdapter.Fill(response);
            return response;
        }
    }

    #endregion [Fill]

    #region [DescribeTableAsync]

    public static List<NpgsqlDbColumn> DescribeTable(string connectionString, string tableName)
        => DescribeTableAsync(connectionString, tableName).GetAwaiter().GetResult();

    public static List<NpgsqlDbColumn> DescribeTable(NpgsqlConnection connection, string tableName)
        => DescribeTableAsync(connection, tableName).GetAwaiter().GetResult();

    public static async Task<List<NpgsqlDbColumn>> DescribeTableAsync(string connectionString, string tableName)
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await DescribeTableAsync(connection, tableName);
        }
    }

    public static async Task<List<NpgsqlDbColumn>> DescribeTableAsync(NpgsqlConnection connection, string tableName)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new NpgsqlCommand($"SELECT * FROM {tableName} WHERE (1=0);", connection))
        {
            command.CommandType = CommandType.Text;
            using (var reader = await command.ExecuteReaderAsync())
            {
                var columns = await reader.GetColumnSchemaAsync();

                return columns.ToList();
            }
        }
    }

    #endregion [DescribeTableAsync]

    #region [BinaryImport]

    public static void BinaryImport<TType>(string connectionString, List<TType> source) where TType : class
        => BinaryImportAsync(connectionString, source).GetAwaiter().GetResult();

    public static void BinaryImport<TType>(string connectionString, List<TType> source, string tableName) where TType : class
        => BinaryImportAsync(connectionString, source, tableName).GetAwaiter().GetResult();

    public static void BinaryImport<TType>(NpgsqlConnection connection, List<TType> source) where TType : class
        => BinaryImportAsync(connection, source).GetAwaiter().GetResult();

    public static void BinaryImport<TType>(NpgsqlConnection connection, List<TType> source, string tableName) where TType : class
        => BinaryImportAsync(connection, source, tableName).GetAwaiter().GetResult();

    public static async Task BinaryImportAsync<TType>(string connectionString, List<TType> source) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            await BinaryImportAsync(connection, source);
        }
    }

    public static async Task BinaryImportAsync<TType>(string connectionString, List<TType> source, string tableName) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            await BinaryImportAsync(connection, source, tableName);
        }
    }

    public static async Task BinaryImportAsync<TType>(NpgsqlConnection connection, List<TType> source) where TType : class
    {
        var tableName = source.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        await BinaryImportAsync(connection, source, tableName);
    }

    public static async Task BinaryImportAsync<TType>(NpgsqlConnection connection, List<TType> source, string tableName) where TType : class
    {
        if (source == null || source.Count == 0)
            return;

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var properties = await MapPropertiesAsync<TType>(connection, tableName);
        properties = properties.Where(t => !t.IgnoreOnInsert).ToList();

        if (properties.Count == 0)
            throw new Exception($"Unable to map object properties with database columns.");

        var columns = string.Join(',', properties.Select(t => t.ColumnName).ToList());
        var commandPrefix = $"COPY {tableName} ({columns}) FROM STDIN (FORMAT BINARY)";

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var binaryImporter = await connection.BeginBinaryImportAsync(commandPrefix))
        {
            foreach (var item in source)
            {
                await binaryImporter.StartRowAsync();

                foreach (var property in properties)
                {
                    var value = property.PropertyInfo.GetValue(item);
                    if (value == null)
                    {
                        await binaryImporter.WriteNullAsync();
                        continue;
                    }
                    
                    await binaryImporter.WriteAsync(value, property.PostgreSqlType);
                }
            }

            await binaryImporter.CompleteAsync();
        }
    }

    #endregion [BinaryImport]

    public static async Task<List<PostgreSqlPropertyDataType>> MapPropertiesAsync<TType>(NpgsqlConnection connection, string tableName) where TType : class
    {
        var properties = typeof(TType).GetProperties().Select(t => new PostgreSqlPropertyDataType(t)).ToList();
        var dbColumns = await DescribeTableAsync(connection, tableName);

        properties = (from a in properties
                      join b in dbColumns on a.ColumnName equals b.ColumnName
                      select a.SetDbInfo(b)).ToList();

        return properties;
    }

    private static async Task<List<PostgreSqlPropertyDataType>> GetResultFieldsAsync<TType>(NpgsqlDataReader dataReader) where TType : class
    {
        var properties = typeof(TType).GetProperties().Select(t => new PostgreSqlPropertyDataType(t)).ToList();
        var columns = await dataReader.GetColumnSchemaAsync();

        var response = (from a in properties
                        join b in columns on a.ColumnName equals b.ColumnName
                        select a.SetDbInfo(b)).ToList();

        return response;
    }

    public static T ConvertDataReaderToClass<T>(NpgsqlDataReader reader, List<PostgreSqlPropertyDataType> resultFields) where T : class
    {
        T response = Activator.CreateInstance<T>();

        foreach (var resultField in resultFields)
        {
            if (reader.IsDBNull(resultField.ColumnIndex))
                continue;

            object? value = null;

            if (resultField.DataType == typeof(Guid))
                value = reader.GetGuid(resultField.ColumnIndex);

            if (resultField.DataType == typeof(Int16))
                value = reader.GetInt16(resultField.ColumnIndex);

            if (resultField.DataType == typeof(Int32))
                value = reader.GetInt32(resultField.ColumnIndex);

            if (resultField.DataType == typeof(Int64))
                value = reader.GetInt64(resultField.ColumnIndex);

            if (resultField.DataType == typeof(byte))
                value = reader.GetByte(resultField.ColumnIndex);

            if (resultField.DataType == typeof(string))
                value = reader.GetString(resultField.ColumnIndex);

            if (resultField.DataType == typeof(decimal))
                value = reader.GetDecimal(resultField.ColumnIndex);

            if (resultField.DataType == typeof(double))
                value = reader.GetDouble(resultField.ColumnIndex);

            if (resultField.DataType == typeof(float))
                value = reader.GetFloat(resultField.ColumnIndex);

            if (resultField.DataType == typeof(DateTime))
                value = reader.GetDateTime(resultField.ColumnIndex);

            if (resultField.DataType == typeof(TimeSpan))
                value = reader.GetTimeSpan(resultField.ColumnIndex);

            if (resultField.DataType == typeof(bool))
                value = reader.GetBoolean(resultField.ColumnIndex);

            var dataType = Nullable.GetUnderlyingType(resultField.PropertyInfo.PropertyType) ?? resultField.PropertyInfo.PropertyType;

            if (dataType == typeof(char[]) && resultField.DataType == typeof(string))
            {
                resultField.PropertyInfo.SetValue(response, value.ToString().ToCharArray(), null);
                continue;
            }

            resultField.PropertyInfo.SetValue(response, value, null);
        }

        return response;
    }
}