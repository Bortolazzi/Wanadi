using System.Data;
using System.Globalization;
using System.Text.Json;
using Npgsql;
using System.Data.Common;
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

    public static async Task<List<DbColumn>> DescribeTableAsync(NpgsqlConnection connection, string tableName, CancellationToken cancellationToken = default)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using (var command = new NpgsqlCommand($"SELECT * FROM {tableName} WHERE (1=0);", connection))
        {
            command.CommandType = CommandType.Text;
            using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                var columns = await reader.GetColumnSchemaAsync(cancellationToken);

                return columns.Cast<DbColumn>().ToList();
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

            var value = ReadValue(reader, resultField);

            resultField.SetValue(response, value);
        }

        return response;
    }

    private static object? ReadValue(NpgsqlDataReader reader, PostgreSqlPropertyDataType resultField)
    {
        var index = resultField.ColumnIndex;
        var targetType = Nullable.GetUnderlyingType(resultField.PropertyInfo.PropertyType) ?? resultField.PropertyInfo.PropertyType;
        var readerType = resultField.DataType;

        if (targetType == typeof(Guid) && readerType == typeof(Guid))
            return reader.GetGuid(index);

        if (targetType == typeof(short) && readerType == typeof(short))
            return reader.GetInt16(index);

        if (targetType == typeof(int) && readerType == typeof(int))
            return reader.GetInt32(index);

        if (targetType == typeof(long) && readerType == typeof(long))
            return reader.GetInt64(index);

        if (targetType == typeof(byte) && readerType == typeof(byte))
            return reader.GetByte(index);

        if (targetType == typeof(string) && readerType == typeof(string))
            return reader.GetString(index);

        if (targetType == typeof(decimal) && readerType == typeof(decimal))
            return reader.GetDecimal(index);

        if (targetType == typeof(double) && readerType == typeof(double))
            return reader.GetDouble(index);

        if (targetType == typeof(float) && readerType == typeof(float))
            return reader.GetFloat(index);

        if (targetType == typeof(DateTime) && readerType == typeof(DateTime))
            return reader.GetFieldValue<DateTime>(index);

        if (targetType == typeof(TimeSpan) && readerType == typeof(TimeSpan))
            return reader.GetFieldValue<TimeSpan>(index);

        if (targetType == typeof(bool) && readerType == typeof(bool))
            return reader.GetBoolean(index);

        if (targetType == typeof(DateOnly) && readerType == typeof(DateOnly))
            return reader.GetFieldValue<DateOnly>(index);

        if (targetType == typeof(TimeOnly) && readerType == typeof(TimeOnly))
            return reader.GetFieldValue<TimeOnly>(index);

        var value = reader.GetValue(index);

        if (targetType == typeof(char[]) && value is string stringValue)
            return stringValue.ToCharArray();

        return ConvertValue(value, targetType);
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null || value is DBNull)
            return null;

        var valueType = value.GetType();
        if (targetType.IsAssignableFrom(valueType))
            return value;

        if (targetType.IsEnum)
        {
            if (value is string enumText)
                return Enum.Parse(targetType, enumText, ignoreCase: true);

            return Enum.ToObject(targetType, value);
        }

        if (targetType == typeof(Guid) && value is string guidText)
            return Guid.Parse(guidText);

        if (targetType == typeof(DateTime) && value is DateOnly dateOnly)
            return dateOnly.ToDateTime(TimeOnly.MinValue);

        if (targetType == typeof(DateOnly) && value is DateTime dateTime)
            return DateOnly.FromDateTime(dateTime);

        if (targetType == typeof(TimeSpan) && value is TimeOnly timeOnly)
            return timeOnly.ToTimeSpan();

        if (targetType == typeof(TimeOnly) && value is TimeSpan timeSpan)
            return TimeOnly.FromTimeSpan(timeSpan);

        if (targetType == typeof(string))
        {
            if (value is JsonDocument jsonDocument)
                return jsonDocument.RootElement.GetRawText();

            if (value is JsonElement jsonElement)
                return jsonElement.GetRawText();

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}
