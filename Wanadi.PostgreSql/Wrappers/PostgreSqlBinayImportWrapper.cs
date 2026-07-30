using System.Data;
using Npgsql;
using Wanadi.Common.Extensions;
using Wanadi.PostgreSql.Contracts;

namespace Wanadi.PostgreSql.Wrappers;

public static partial class PostgreSqlWrapper
{
    public static void BinaryImport<TType>(string connectionString, List<TType> source) where TType : class
        => BinaryImportAsync(connectionString, source).GetAwaiter().GetResult();

    public static void BinaryImport<TType>(string connectionString, List<TType> source, string tableName) where TType : class
        => BinaryImportAsync(connectionString, source, tableName).GetAwaiter().GetResult();

    public static void BinaryImport<TType>(NpgsqlConnection connection, List<TType> source) where TType : class
        => BinaryImportAsync(connection, source).GetAwaiter().GetResult();

    public static void BinaryImport<TType>(NpgsqlConnection connection, List<TType> source, string tableName) where TType : class
        => BinaryImportAsync(connection, source, tableName).GetAwaiter().GetResult();

    public static async Task BinaryImportAsync<TType>(string connectionString, List<TType> source, CancellationToken cancellationToken = default) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            await BinaryImportAsync(connection, source, cancellationToken);
        }
    }

    public static async Task BinaryImportAsync<TType>(string connectionString, List<TType> source, string tableName, CancellationToken cancellationToken = default) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString, cancellationToken))
        {
            await BinaryImportAsync(connection, source, tableName, cancellationToken);
        }
    }

    public static async Task BinaryImportAsync<TType>(NpgsqlConnection connection, List<TType> source, CancellationToken cancellationToken = default) where TType : class
    {
        var tableName = source.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        await BinaryImportAsync(connection, source, tableName, cancellationToken);
    }

    public static async Task BinaryImportAsync<TType>(NpgsqlConnection connection, List<TType> source, string tableName, CancellationToken cancellationToken = default) where TType : class
    {
        if (source == null || source.Count == 0)
            return;

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var properties = await MapPropertiesAsync<TType>(connection, tableName, cancellationToken);

        await BinaryImportAsync(connection, source, tableName, properties, cancellationToken);
    }

    internal static async Task BinaryImportAsync<TType>(
        NpgsqlConnection connection,
        List<TType> source,
        string tableName,
        List<PostgreSqlPropertyDataType> properties,
        CancellationToken cancellationToken = default) where TType : class
    {
        if (source is null || source.Any() is false)
            return;

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        properties = properties.Where(t => !t.IgnoreOnInsert).ToList();

        if (properties.Any() is false)
            throw new Exception($"Unable to map object properties with database columns.");

        var columns = string.Join(',', properties.Select(t => $"\"{t.ColumnName}\"").ToList());
        var commandPrefix = $"COPY {tableName} ({columns}) FROM STDIN (FORMAT BINARY)";

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using (var binaryImporter = await connection.BeginBinaryImportAsync(commandPrefix, cancellationToken))
        {
            foreach (var item in source)
            {
                await binaryImporter.StartRowAsync(cancellationToken);

                foreach (var property in properties)
                {
                    var value = property.PropertyInfo.GetValue(item);
                    if (value == null)
                    {
                        await binaryImporter.WriteNullAsync(cancellationToken);
                        continue;
                    }

                    await binaryImporter.WriteAsync(value, property.PostgreSqlType, cancellationToken);
                }
            }

            await binaryImporter.CompleteAsync(cancellationToken);
        }
    }
}
