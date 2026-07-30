using System.Data;
using Npgsql;
using Npgsql.Schema;
using Wanadi.PostgreSql.Contracts;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    #region [ConnectionString]

    public static string BuildConnectionString(PostgreSqlConnectionSettings settings, string database)
        => BuildConnectionString(
            settings.Host,
            settings.Username,
            settings.Password,
            database,
            settings.Port,
            settings.Timeout,
            settings.CommandTimeout,
            settings.MaxPoolSize,
            settings.CancellationTimeout,
            settings.KeepAlive,
            settings.IncludeErrorDetail);

    public static string BuildConnectionString(
        string host,
        string username,
        string password,
        string? database,
        int? port = 5432,
        int? timeout = 15,
        int? commandTimeout = 30,
        int? maxPoolSize = 5000,
        int? cancellationTimeout = 2000,
        int? keepAlive = 0,
        bool? includeErrorDetail = false)
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
            KeepAlive = keepAlive ?? 0,
            MaxPoolSize = maxPoolSize ?? 5000,
            CancellationTimeout = cancellationTimeout ?? 2000,
            IncludeErrorDetail = includeErrorDetail ?? false
        };

        return builder.ConnectionString;
    }

    #endregion [ConnectionString]

    #region [GetConnectionAsync]

    public static NpgsqlConnection GetConnection(string connectionString)
        => GetConnectionAsync(connectionString).GetAwaiter().GetResult();

    public static async Task<NpgsqlConnection> GetConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    #endregion [GetConnectionAsync]

    #region [DescribeTableAsync]

    public static async Task<List<NpgsqlDbColumn>> DescribeTableAsync(NpgsqlConnection connection, string tableName, CancellationToken cancellationToken = default)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using (var command = new NpgsqlCommand($"SELECT * FROM {tableName} WHERE (1=0);", connection))
        {
            command.CommandType = CommandType.Text;
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                var columns = await reader.GetColumnSchemaAsync(cancellationToken);

                return columns.ToList();
            }
        }
    }

    #endregion [DescribeTableAsync]

    public static async Task<List<PostgreSqlPropertyDataType>> MapPropertiesAsync<TType>(NpgsqlConnection connection, string tableName, CancellationToken cancellationToken = default) where TType : class
    {
        var properties = typeof(TType).GetProperties().Select(t => new PostgreSqlPropertyDataType(t)).ToList();
        var dbColumns = await DescribeTableAsync(connection, tableName, cancellationToken);

        properties = (from a in properties
                      join b in dbColumns on a.ColumnName equals b.ColumnName
                      select a.SetDbInfo(b)).ToList();

        return properties;
    }

    public static async Task<List<PostgreSqlPropertyDataType>> GetResultFieldsAsync<TType>(NpgsqlDataReader dataReader, CancellationToken cancellationToken = default) where TType : class
    {
        var properties = typeof(TType).GetProperties().Select(t => new PostgreSqlPropertyDataType(t)).ToList();
        var columns = await dataReader.GetColumnSchemaAsync(cancellationToken);

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
