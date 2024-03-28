using System.Data;
using ClickHouse.Ado;
using ClickHouse.Client.Copy;
using Wanadi.Clickhouse.Contracts;
using Wanadi.Common.Contracts.PropertyMappers;
using Wanadi.Common.Extensions;
using Wanadi.Common.Helpers;

namespace Wanadi.Clickhouse.Wrappers;

public static class ClickHouseWrapper
{
    public static bool ForceMaxConnectionTimeout = false;

    public static ClickHouseConnectionSettings BuildConnectionSettings(string host, string database, string user, string? password, int port = 9000, int bufferSize = 32768, int socketTimeout = 10000)
    {
        var settings = new ClickHouseConnectionSettings();
        settings.Host = host;
        settings.Database = database;
        settings.User = user;
        settings.Port = port;

        if (password is not null)
            settings.Password = password;

        settings.Compress = true;
        settings.Compressor = "lz4";
        settings.BufferSize = bufferSize;
        settings.SocketTimeout = socketTimeout;
        settings.CheckCompressedHash = false;
        settings.Encrypt = false;
        settings.Async = true;

        return settings;
    }

    public static string BuildConnectionString(string host, string database, string user, string? password, int port = 9000, int bufferSize = 32768, int socketTimeout = 10000)
        => BuildConnectionSettings(host, database, user, password, port, bufferSize, socketTimeout).ToString();

    public static async Task<ClickHouseConnection> GetConnectionAsync(ClickHouseConnectionSettings settings)
    {
        var connection = new ClickHouseConnection(settings);
        await connection.OpenAsync();
        return connection;
    }

    #region [ExecuteNonQuery]

    public static int ExecuteNonQuery(ClickHouseConnectionSettings settings, string commandExecute)
        => ExecuteNonQueryAsync(settings, commandExecute).GetAwaiter().GetResult();

    public static int ExecuteNonQuery(ClickHouseConnection connection, string commandExecute)
        => ExecuteNonQueryAsync(connection, commandExecute).GetAwaiter().GetResult();

    public static async Task<int> ExecuteNonQueryAsync(ClickHouseConnectionSettings settings, string commandExecute)
    {
        using (var connection = await GetConnectionAsync(settings))
        {
            return await ExecuteNonQueryAsync(connection, commandExecute);
        }
    }

    public static async Task<int> ExecuteNonQueryAsync(ClickHouseConnection connection, string commandExecute)
    {
        using (var command = connection.CreateCommand(commandExecute))
        {
            command.CommandType = CommandType.Text;
            return await command.ExecuteNonQueryAsync();
        }
    }

    #endregion [ExecuteNonQuery]

    #region [ExecuteScalar]

    public static object? ExecuteScalar(ClickHouseConnectionSettings settings, string commandExecute)
        => ExecuteScalarAsync(settings, commandExecute).GetAwaiter().GetResult();

    public static object? ExecuteScalar(ClickHouseConnection connection, string commandExecute)
        => ExecuteScalarAsync(connection, commandExecute).GetAwaiter().GetResult();

    public static async Task<object?> ExecuteScalarAsync(ClickHouseConnectionSettings settings, string commandExecute)
    {
        using (var connection = await GetConnectionAsync(settings))
        {
            return await ExecuteScalarAsync(connection, commandExecute);
        }
    }

    public static async Task<object?> ExecuteScalarAsync(ClickHouseConnection connection, string commandExecute)
    {
        using (var command = connection.CreateCommand(commandExecute))
        {
            command.CommandType = CommandType.Text;
            return await command.ExecuteScalarAsync();
        }
    }

    #endregion [ExecuteScalar]

    #region [SelectQuery]

    public static List<TType> SelectQuery<TType>(ClickHouseConnectionSettings settings, string commandQuery) where TType : class
        => SelectQueryAsync<TType>(settings, commandQuery).GetAwaiter().GetResult();

    public static List<TType> SelectQuery<TType>(ClickHouseConnection connection, string commandQuery) where TType : class
        => SelectQueryAsync<TType>(connection, commandQuery).GetAwaiter().GetResult();

    public static async Task<List<TType>> SelectQueryAsync<TType>(ClickHouseConnectionSettings settings, string commandQuery) where TType : class
    {
        using (var connection = await GetConnectionAsync(settings))
        {
            return await SelectQueryAsync<TType>(connection, commandQuery);
        }
    }

    public static async Task<List<TType>> SelectQueryAsync<TType>(ClickHouseConnection connection, string commandQuery) where TType : class
    {
        using (var command = connection.CreateCommand(commandQuery))
        {
            command.CommandType = CommandType.Text;

            using (var reader = (ClickHouseDataReader)await command.ExecuteReaderAsync())
            {
                var resultFields = GetResultFields<TType>(reader);
                var response = new List<TType>();

                do
                {
                    while (await reader.ReadAsync())
                    {
                        response.Add(ConvertDataReaderToClass<TType>(reader, resultFields));
                    }
                }
                while (await reader.NextResultAsync());

                return response;
            }
        }
    }

    #endregion [SelectQuery]

    #region [SelectQueryFirstOrDefault]

    public static TType? SelectQueryFirstOrDefault<TType>(ClickHouseConnectionSettings settings, string commandQuery) where TType : class
        => SelectQueryFirstOrDefaultAsync<TType>(settings, commandQuery).GetAwaiter().GetResult();

    public static TType? SelectQueryFirstOrDefault<TType>(ClickHouseConnection connection, string commandQuery) where TType : class
        => SelectQueryFirstOrDefaultAsync<TType>(connection, commandQuery).GetAwaiter().GetResult();

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(ClickHouseConnectionSettings settings, string commandQuery) where TType : class
    {
        using (var connection = await GetConnectionAsync(settings))
        {
            return await SelectQueryFirstOrDefaultAsync<TType>(connection, commandQuery);
        }
    }

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(ClickHouseConnection connection, string commandQuery) where TType : class
    {
        using (var command = connection.CreateCommand(commandQuery))
        {
            command.CommandType = CommandType.Text;

            using (var reader = (ClickHouseDataReader)await command.ExecuteReaderAsync())
            {
                var resultFields = GetResultFields<TType>(reader);
                var response = new List<TType>();

                do
                {
                    while (await reader.ReadAsync())
                    {
                        return ConvertDataReaderToClass<TType>(reader, resultFields);
                    }
                }
                while (await reader.NextResultAsync());
            }
        }

        return null;
    }

    #endregion [SelectQueryFirstOrDefault]

    #region [Batch Insert]

    public static async Task BatchInsertAsync<TType>(ClickHouseConnection connection, List<TType> sourceItems, int batchQuantity = 100000) where TType : class
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return;

        var tableName = sourceItems.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        await BatchInsertAsync<TType>(connection, tableName, sourceItems, batchQuantity);
    }

    public static async Task BatchInsertAsync<TType>(ClickHouseConnection connection, string tableName, List<TType> sourceItems, int batchQuantity = 100000) where TType : class
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return;

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var prefixCommand = GetBatchInsertCommandPrefix(sourceItems.GetType().GetGenericArguments()[0], tableName);

        var iterations = MathHelper.CalculateIterations(sourceItems.Count, batchQuantity);

        for (int i = 0; i < iterations; i++)
        {
            $"Running batch command {(i + 1):N0} of {iterations:N0}".PrintInfo();

            var sourceBatch = sourceItems.Skip(i * batchQuantity).Take(batchQuantity).ToList();

            using (var command = connection.CreateCommand(prefixCommand))
            {
                command.Parameters.Add(new ClickHouseParameter()
                {
                    ParameterName = "batch",
                    Value = sourceBatch
                });

                await command.ExecuteNonQueryAsync();
                sourceBatch.Clear();
            }
        }
    }

    #endregion [Batch Insert]

    #region [Bulk Insert]

    public static async Task BulkInsertAsync<TType>(ClickHouseConnectionSettings settings, List<TType> sourceItems, bool isHttps = false) where TType : class
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return;

        var tableName = sourceItems.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        await BulkInsertAsync(settings, tableName, sourceItems, isHttps);
    }

    public static async Task BulkInsertAsync<TType>(ClickHouseConnectionSettings settings, string tableName, List<TType> sourceItems, bool isHttps = false) where TType : class
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return;

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var propertiesToInsert = GetPropertiesToInsert(typeof(TType));
        var values = ConvertListToBulkInsert(sourceItems, propertiesToInsert);

        var connectionString = new ClickHouse.Client.ADO.ClickHouseConnectionStringBuilder();
        connectionString.Host = settings.Host;
        connectionString.Database = settings.Database;
        connectionString.Username = settings.User;
        connectionString.Port = isHttps ? (ushort)8443 : (ushort)8123;
        connectionString.Timeout = TimeSpan.FromMinutes(30);
        
        if (settings.Password is not null)
            connectionString.Password = settings.Password;

        connectionString.Compression = true;
        connectionString.UseServerTimezone = true;
        
        var connection = new ClickHouse.Client.ADO.ClickHouseConnection(connectionString.ToString());

        using var bulkCopy = new ClickHouseBulkCopy(connection)
        {
            DestinationTableName = $"{settings.Database}.{tableName}",
            ColumnNames = propertiesToInsert.Select(t => t.ColumnName).ToArray(),
            BatchSize = 100000
        };

        await bulkCopy.InitAsync();

        await bulkCopy.WriteToServerAsync(values);
    }

    #endregion [Bulk Insert]

    private static IEnumerable<object[]> ConvertListToBulkInsert<TType>(List<TType> sourceItems, List<PropertyDataType> properties) where TType : class
    {
        var response = new List<object[]>();

        foreach (var item in sourceItems)
        {
            var propertyValues = new List<object>();

            foreach (var prop in properties)
            {
                propertyValues.Add(prop.PropertyInfo.GetValue(item));
            }

            response.Add(propertyValues.ToArray());
            propertyValues.Clear();
        }

        return response;
    }

    private static string GetBatchInsertCommandPrefix(Type objectType, string tableName)
    {
        var properties = GetPropertiesToInsert(objectType);

        return $"INSERT INTO {tableName} ({string.Join(", ", properties.Select(t => t.ColumnName).ToList())}) VALUES @batch";
    }

    private static List<PropertyDataType> GetPropertiesToInsert(Type objectType)
    {
        var properties = objectType.GetProperties().Select(t => new PropertyDataType(t)).ToList();
        return properties.Where(t => !t.IgnoreOnInsert).OrderBy(t => t.ColumnName).ToList();
    }

    private static List<ClickHouseColumnsResultDataType> GetResultFields<TType>(ClickHouseDataReader dataReader) where TType : class
    {
        var properties = typeof(TType).GetProperties().Select(t => new PropertyDataType(t)).ToList();

        var response = new List<ClickHouseColumnsResultDataType>();

        for (var i = 0; i < dataReader.FieldCount; i++)
        {
            var dataReaderField = new ClickHouseColumnsResultDataType(dataReader, i);

            var property = properties.FirstOrDefault(t => t.ColumnName == dataReaderField.ColumnName);
            if (property != null)
                response.Add(dataReaderField.SetPropertyInfo(property.PropertyInfo));
        }

        return response.Where(t => t.Property != null).ToList();
    }

    private static T ConvertDataReaderToClass<T>(ClickHouseDataReader reader, List<ClickHouseColumnsResultDataType> resultFields) where T : class
    {
        T response = Activator.CreateInstance<T>();

        foreach (var resultField in resultFields)
        {
            object? value = reader[resultField.ColumnIndex];

            if (value == null || value == DBNull.Value)
                continue;

            var dataType = Nullable.GetUnderlyingType(resultField.Property.PropertyType) ?? resultField.Property.PropertyType;

            if (dataType == typeof(Guid))
            {
                resultField.Property.SetValue(response, Guid.Parse(value.ToString()), null);
                continue;
            }

            if (dataType == typeof(DateTime) || dataType == typeof(Boolean))
            {
                resultField.Property.SetValue(response, Convert.ChangeType(value, dataType), null);
                continue;
            }

            resultField.Property.SetValue(response, value, null);
        }

        return response;
    }
}