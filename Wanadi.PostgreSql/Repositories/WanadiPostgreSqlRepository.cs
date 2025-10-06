using Npgsql;
using Wanadi.Common.Extensions;
using Wanadi.PostgreSql.Contracts;
using Wanadi.PostgreSql.Interfaces;
using Wanadi.PostgreSql.Wrappers;

namespace Wanadi.PostgreSql.Repositories;

public abstract class WanadiPostgreSqlRepository<TEntity> : IWanadiPostgreSqlRepository<TEntity> where TEntity : class, new()
{
    private NpgsqlConnection? _connection { get; set; }
    readonly string _connectionString;

    public WanadiPostgreSqlRepository(string connectionString)
        => _connectionString = connectionString;

    public WanadiPostgreSqlRepository(PostgreSqlConnectionSettings settings, string databaseName)
        => _connectionString = PostgreSqlWrapper.BuildConnectionString(settings, databaseName);

    public string GetTableName()
        => typeof(TEntity).GetTableName();

    public async Task<TEntity?> AddAsync(TEntity entity, string? tableName = null)
    {
        if (entity is null)
            return null;

        if (entity == default(TEntity))
            return null;

        tableName = tableName ?? GetTableName();

        var properties = await PostgreSqlWrapper.MapPropertiesAsync<TEntity>(await GetConnectionAsync(), tableName);

        if (properties.Count(t => t.HasKeyAttribute) == 0)
            throw new Exception($"Entity does not have an identifier defined in the properties. (KeyAttribute)");

        if (properties.Count(t => t.HasKeyAttribute) > 1)
            throw new Exception($"Entity has more than one identifying property. Method only allows one. (KeyAttribute)");

        var identifier = properties.FirstOrDefault(t => t.HasKeyAttribute);

        properties = properties.Where(t => !t.IgnoreOnInsert).ToList();
        if (properties.Count == 0)
            throw new Exception($"Unable to identify entity properties.");

        var parameters = new List<NpgsqlParameter>();

        foreach (var property in properties)
        {
            var value = property.PropertyInfo.GetValue(entity);
            if (value == null && !property.AllowNull)
                throw new Exception($"Property {property.Name} does not allow null values.");

            if (value == null)
                value = DBNull.Value;

            var parameterToAdd = new NpgsqlParameter($"@param_{property.ColumnName}", property.PostgreSqlType);
            parameterToAdd.Value = value;
            parameters.Add(parameterToAdd);
        }

        var commandToExecute = $"INSERT INTO {tableName} ({string.Join(", ", properties.Select(t => $"\"{t.ColumnName}\"").ToList())}) VALUES ({string.Join(", ", properties.Select(t => $"@param_{t.ColumnName}").ToList())}) returning \"{identifier.ColumnName}\";";

        var idValue = await PostgreSqlWrapper.ExecuteScalarAsync(await GetConnectionAsync(), commandToExecute, parameters);

        identifier.PropertyInfo.SetValue(entity, idValue);

        return entity;
    }

    public async Task<TEntity?> GetByIdAsync(int id)
        => await FirstOrDefaultAsync($"SELECT * FROM {GetTableName()} WHERE id = {id};");

    public async Task DeleteByIdAsync(int id)
        => await ExecuteNonQueryAsync($"DELETE FROM {GetTableName()} WHERE id = {id};");

    public async Task UpdateAsync(TEntity entity, string? tableName = null)
    {
        if (entity is null)
            return;

        if (entity == default(TEntity))
            return;

        tableName = tableName ?? GetTableName();

        var properties = await PostgreSqlWrapper.MapPropertiesAsync<TEntity>(await GetConnectionAsync(), tableName);

        if (properties.Count(t => t.HasKeyAttribute) == 0)
            throw new Exception($"Entity does not have an identifier defined in the properties. (KeyAttribute)");

        if (properties.Count(t => t.HasKeyAttribute) > 1)
            throw new Exception($"Entity has more than one identifying property. Method only allows one. (KeyAttribute)");

        var identifier = properties.FirstOrDefault(t => t.HasKeyAttribute);

        properties = properties.Where(t => !t.IgnoreOnInsert).ToList();
        if (properties.Count == 0)
            throw new Exception($"Unable to identify entity properties.");

        var parameters = new List<NpgsqlParameter>();

        var identifierParameter = new NpgsqlParameter($"@param_{identifier.ColumnName}", identifier.PostgreSqlType);
        identifierParameter.Value = identifier.PropertyInfo.GetValue(entity);

        parameters.Add(identifierParameter);

        foreach (var property in properties)
        {
            var value = property.PropertyInfo.GetValue(entity);
            if (value == null && !property.AllowNull)
                throw new Exception($"Property {property.Name} does not allow null values.");

            if (value == null)
                value = DBNull.Value;

            var parameterToAdd = new NpgsqlParameter($"@param_{property.ColumnName}", property.PostgreSqlType);
            parameterToAdd.Value = value;
            parameters.Add(parameterToAdd);
        }

        var commandToExecute = $"UPDATE {tableName} SET {string.Join(", ", properties.Select(t => $"\"{t.ColumnName}\" = @param_{t.ColumnName}").ToList())} WHERE \"{identifier.ColumnName}\" = @param_{identifier.ColumnName};";
        await PostgreSqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), commandToExecute, parameters);
    }

    public async Task<List<TEntity>> ToListAsync()
        => await PostgreSqlWrapper.SelectQueryByEntityAsync<TEntity>(await GetConnectionAsync());

    public async Task<List<TEntity>> SelectQueryAsync(string query, List<NpgsqlParameter>? parameters = null)
        => await PostgreSqlWrapper.SelectQueryAsync<TEntity>(await GetConnectionAsync(), query, parameters);

    public async Task<List<T>> SelectQueryAsync<T>(string query, List<NpgsqlParameter>? parameters = null) where T : class
        => await PostgreSqlWrapper.SelectQueryAsync<T>(await GetConnectionAsync(), query, parameters);

    public async Task<TEntity?> FirstOrDefaultAsync(string query, List<NpgsqlParameter>? parameters = null)
        => await PostgreSqlWrapper.SelectQueryFirstOrDefaultAsync<TEntity>(await GetConnectionAsync(), query, parameters);

    public async Task<T?> FirstOrDefaultAsync<T>(string query, List<NpgsqlParameter>? parameters = null) where T : class
        => await PostgreSqlWrapper.SelectQueryFirstOrDefaultAsync<T>(await GetConnectionAsync(), query, parameters);

    public async Task BinaryImportAsync(List<TEntity> entities)
        => await PostgreSqlWrapper.BinaryImportAsync(await GetConnectionAsync(), entities);

    public async Task<int> ExecuteNonQueryAsync(string query, List<NpgsqlParameter>? parameters = null)
        => await PostgreSqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), query, parameters);

    public async Task<object?> ExecuteScalarAsync(string query, List<NpgsqlParameter>? parameters = null)
        => await PostgreSqlWrapper.ExecuteScalarAsync(await GetConnectionAsync(), query, parameters);

    public async Task TruncateTableAsync()
        => await PostgreSqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), $"TRUNCATE TABLE {GetTableName()};");

    public async Task ResetIdentityTableAsync()
        => await PostgreSqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), $"TRUNCATE TABLE {GetTableName()} RESTART IDENTITY;");

    public async Task<long> CountAsync()
    {
        var response = await ExecuteScalarAsync($"SELECT COUNT(1) FROM {GetTableName()};");
        if (response == null)
            return 0;

        return Convert.ToInt64(response);
    }

    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            return _connection;

        _connection = await PostgreSqlWrapper.GetConnectionAsync(_connectionString);
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}