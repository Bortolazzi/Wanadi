using System.Data;
using Npgsql;
using Wanadi.Common.Extensions;

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

        var columns = string.Join(',', properties.Select(t => $"\"{t.ColumnName}\"").ToList());
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
}