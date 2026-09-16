using System.Collections.Concurrent;
using Npgsql;
using Wanadi.Common.Extensions;
using Wanadi.PostgreSql.Contracts;
using Wanadi.PostgreSql.Interfaces;
using Wanadi.PostgreSql.Wrappers;

namespace Wanadi.PostgreSql.Repositories;

public abstract class WanadiPostgreSqlRepository<TEntity> : IWanadiPostgreSqlRepository<TEntity> where TEntity : class, new()
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ConcurrentDictionary<string, List<PostgreSqlPropertyDataType>> _propertyMapCache = new();

    public WanadiPostgreSqlRepository(NpgsqlDataSource dataSource)
        => _dataSource = dataSource;

    public string GetTableName()
        => typeof(TEntity).GetTableName();

    public async Task<TEntity?> AddAsync(TEntity entity, string? tableName = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return null;

        if (entity == default(TEntity))
            return null;

        tableName = tableName ?? GetTableName();

        await using var connection = await GetConnectionAsync(cancellationToken);

        var properties = await GetMappedPropertiesAsync(connection, tableName, cancellationToken);

        if (properties.Any(t => t.HasKeyAttribute) is false)
            throw new Exception($"Entity does not have an identifier defined in the properties. (KeyAttribute)");

        if (properties.Count(t => t.HasKeyAttribute) > 1)
            throw new Exception($"Entity has more than one identifying property. Method only allows one. (KeyAttribute)");

        var identifier = properties.FirstOrDefault(t => t.HasKeyAttribute);

        properties = properties.Where(t => !t.IgnoreOnInsert).ToList();
        if (properties.Any() is false)
            throw new Exception($"Unable to identify entity properties.");

        var parameters = new List<NpgsqlParameter>();

        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            if (value == null && !property.AllowNull)
                throw new Exception($"Property {property.Name} does not allow null values.");

            if (value == null)
                value = DBNull.Value;

            var parameterToAdd = new NpgsqlParameter($"@param_{property.ColumnName}", value);
            if (property.PostgreSqlType.HasValue)
                parameterToAdd.NpgsqlDbType = property.PostgreSqlType.Value;

            parameterToAdd.Value = value;
            parameters.Add(parameterToAdd);
        }

        var commandToExecute = $"INSERT INTO {tableName} ({string.Join(", ", properties.Select(t => $"\"{t.ColumnName}\"").ToList())}) VALUES ({string.Join(", ", properties.Select(t => $"@param_{t.ColumnName}").ToList())}) returning \"{identifier.ColumnName}\";";

        var idValue = await PostgreSqlWrapper.ExecuteScalarAsync(connection, commandToExecute, parameters, cancellationToken);

        identifier.SetValue(entity, idValue);

        return entity;
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await GetByIdAsync<int>(id, cancellationToken);

    public async Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
    {
        var tableName = GetTableName();

        await using var connection = await GetConnectionAsync(cancellationToken);

        var properties = await GetMappedPropertiesAsync(connection, tableName, cancellationToken);
        var identifier = GetIdentifier(properties);
        var parameter = CreateParameter(identifier, id);
        var query = $"SELECT * FROM {tableName} WHERE \"{identifier.ColumnName}\" = @param_{identifier.ColumnName};";

        return await PostgreSqlWrapper.QueryFirstOrDefaultAsync<TEntity>(connection, query, new List<NpgsqlParameter> { parameter }, cancellationToken);
    }

    public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DeleteByIdAsync<int>(id, cancellationToken);

    public async Task DeleteByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
    {
        var tableName = GetTableName();

        await using var connection = await GetConnectionAsync(cancellationToken);

        var properties = await GetMappedPropertiesAsync(connection, tableName, cancellationToken);
        var identifier = GetIdentifier(properties);
        var parameter = CreateParameter(identifier, id);
        var query = $"DELETE FROM {tableName} WHERE \"{identifier.ColumnName}\" = @param_{identifier.ColumnName};";

        await PostgreSqlWrapper.ExecuteNonQueryAsync(connection, query, new List<NpgsqlParameter> { parameter }, cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, string? tableName = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return;

        if (entity == default(TEntity))
            return;

        tableName = tableName ?? GetTableName();

        await using var connection = await GetConnectionAsync(cancellationToken);

        var properties = await GetMappedPropertiesAsync(connection, tableName, cancellationToken);

        if (properties.Any(t => t.HasKeyAttribute) is false)
            throw new Exception($"Entity does not have an identifier defined in the properties. (KeyAttribute)");

        if (properties.Count(t => t.HasKeyAttribute) > 1)
            throw new Exception($"Entity has more than one identifying property. Method only allows one. (KeyAttribute)");

        var identifier = properties.FirstOrDefault(t => t.HasKeyAttribute);

        properties = properties.Where(t => !t.IgnoreOnInsert && !t.HasKeyAttribute).ToList();
        if (properties.Any() is false)
            throw new Exception($"Unable to identify entity properties.");

        var parameters = new List<NpgsqlParameter>();

        var identifierValue = identifier.GetValue(entity);
        var identifierParameter = new NpgsqlParameter($"@param_{identifier.ColumnName}", identifierValue);
        if (identifier.PostgreSqlType.HasValue)
            identifierParameter.NpgsqlDbType = identifier.PostgreSqlType.Value;

        identifierParameter.Value = identifierValue;

        parameters.Add(identifierParameter);

        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            if (value == null && !property.AllowNull)
                throw new Exception($"Property {property.Name} does not allow null values.");

            if (value == null)
                value = DBNull.Value;

            var parameterToAdd = new NpgsqlParameter($"@param_{property.ColumnName}", value);
            if (property.PostgreSqlType.HasValue)
                parameterToAdd.NpgsqlDbType = property.PostgreSqlType.Value;

            parameterToAdd.Value = value;
            parameters.Add(parameterToAdd);
        }

        var commandToExecute = $"UPDATE {tableName} SET {string.Join(", ", properties.Select(t => $"\"{t.ColumnName}\" = @param_{t.ColumnName}").ToList())} WHERE \"{identifier.ColumnName}\" = @param_{identifier.ColumnName};";
        await PostgreSqlWrapper.ExecuteNonQueryAsync(connection, commandToExecute, parameters, cancellationToken);
    }

    public async Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.QueryByEntityAsync<TEntity>(connection, cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> QueryAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.QueryAsync<TEntity>(connection, query, parameters, cancellationToken);
    }

    public async Task<List<T>> QueryAsync<T>(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where T : class
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.QueryAsync<T>(connection, query, parameters, cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.QueryFirstOrDefaultAsync<TEntity>(connection, query, parameters, cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync<T>(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where T : class
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.QueryFirstOrDefaultAsync<T>(connection, query, parameters, cancellationToken);
    }

    public async Task BinaryImportAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || entities.Count == 0)
            return;

        var tableName = entities.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        await using var connection = await GetConnectionAsync(cancellationToken);

        var properties = await GetMappedPropertiesAsync(connection, tableName, cancellationToken);

        await PostgreSqlWrapper.BinaryImportAsync(connection, entities, tableName, properties, cancellationToken);
    }

    public async Task<int> ExecuteNonQueryAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.ExecuteNonQueryAsync(connection, query, parameters, cancellationToken);
    }

    public async Task<object?> ExecuteScalarAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        return await PostgreSqlWrapper.ExecuteScalarAsync(connection, query, parameters, cancellationToken);
    }

    public async Task TruncateTableAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        await PostgreSqlWrapper.ExecuteNonQueryAsync(connection, $"TRUNCATE TABLE {GetTableName()};", cancellationToken: cancellationToken);
    }

    public async Task ResetIdentityTableAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);
        await PostgreSqlWrapper.ExecuteNonQueryAsync(connection, $"TRUNCATE TABLE {GetTableName()} RESTART IDENTITY;", cancellationToken: cancellationToken);
    }

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteScalarAsync($"SELECT COUNT(1) FROM {GetTableName()};", cancellationToken: cancellationToken);
        if (response == null)
            return 0;

        return Convert.ToInt64(response);
    }

    public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        => await _dataSource.OpenConnectionAsync(cancellationToken);

    private async Task<List<PostgreSqlPropertyDataType>> GetMappedPropertiesAsync(NpgsqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        if (_propertyMapCache.TryGetValue(tableName, out var properties))
            return properties;

        properties = await PostgreSqlWrapper.MapPropertiesAsync<TEntity>(connection, tableName, cancellationToken);

        return _propertyMapCache.GetOrAdd(tableName, properties);
    }

    private static PostgreSqlPropertyDataType GetIdentifier(List<PostgreSqlPropertyDataType> properties)
    {
        if (properties.Any(t => t.HasKeyAttribute) is false)
            throw new Exception($"Entity does not have an identifier defined in the properties. (KeyAttribute)");

        if (properties.Count(t => t.HasKeyAttribute) > 1)
            throw new Exception($"Entity has more than one identifying property. Method only allows one. (KeyAttribute)");

        return properties.First(t => t.HasKeyAttribute);
    }

    private static NpgsqlParameter CreateParameter<TValue>(PostgreSqlPropertyDataType property, TValue value)
    {
        var parameter = new NpgsqlParameter($"@param_{property.ColumnName}", value ?? (object)DBNull.Value);
        if (property.PostgreSqlType.HasValue)
            parameter.NpgsqlDbType = property.PostgreSqlType.Value;

        parameter.Value = value ?? (object)DBNull.Value;

        return parameter;
    }

    public void Dispose()
    {
    }
}
