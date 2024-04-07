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

    public async Task AddAsync(TEntity entity)
    {
        var source = new List<TEntity>() { entity };
        await BinaryImportAsync(source);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        if (entity is null)
            return;

        if (entity == default(TEntity))
            return;

        var properties = await PostgreSqlWrapper.MapPropertiesAsync<TEntity>(await GetConnectionAsync(), GetTableName());

        if (properties.Count(t => t.HasKeyAttribute) == 0)
            throw new Exception($"Entity does not have an identifier defined in the properties. (KeyAttribute)");

        if (properties.Count(t => t.HasKeyAttribute) > 1)
            throw new Exception($"Entity has more than one identifying property. Method only allows one. (KeyAttribute)");

        var identifier = properties.FirstOrDefault(t => t.HasKeyAttribute);

        properties = properties.Where(t => !t.IgnoreOnInsert).ToList();
        if (properties.Count == 0)
            throw new Exception($"Unable to identify entity properties.");

        var parameters = new List<NpgsqlParameter>();

        var identifierParameter = new NpgsqlParameter($"@{identifier.ColumnName}", identifier.PostgreSqlType);
        identifierParameter.Value = identifier.PropertyInfo.GetValue(entity);

        parameters.Add(identifierParameter);

        foreach (var property in properties)
        {
            var value = property.PropertyInfo.GetValue(entity);
            if (value == null && !property.AllowNull)
                throw new Exception($"Property {property.Name} does not allow null values.");

            if (value == null)
                value = DBNull.Value;

            var parameterToAdd = new NpgsqlParameter($"@{property.ColumnName}", property.PostgreSqlType);
            parameterToAdd.Value = value;
            parameters.Add(parameterToAdd);
        }

        var commandToExecute = $"UPDATE {GetTableName()} SET {string.Join(", ", properties.Select(t=> $"{t.ColumnName} = @{t.ColumnName}").ToList())} WHERE {identifier.ColumnName} = @{identifier.ColumnName};";
        await PostgreSqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), commandToExecute, parameters);
    }

    public async Task<List<TEntity>> ToListAsync()
        => await PostgreSqlWrapper.SelectQueryByEntityAsync<TEntity>(await GetConnectionAsync());

    public async Task<List<TEntity>> SelectQueryAsync(string query)
        => await PostgreSqlWrapper.SelectQueryAsync<TEntity>(await GetConnectionAsync(), query);

    public async Task<List<T>> SelectQueryAsync<T>(string query) where T : class
        => await PostgreSqlWrapper.SelectQueryAsync<T>(await GetConnectionAsync(), query);

    public async Task<TEntity?> FirstOrDefaultAsync(string query)
        => await PostgreSqlWrapper.SelectQueryFirstOrDefaultAsync<TEntity>(await GetConnectionAsync(), query);

    public async Task<T?> FirstOrDefaultAsync<T>(string query) where T : class
        => await PostgreSqlWrapper.SelectQueryFirstOrDefaultAsync<T>(await GetConnectionAsync(), query);

    public async Task BinaryImportAsync(List<TEntity> entities)
        => await PostgreSqlWrapper.BinaryImportAsync(await GetConnectionAsync(), entities);

    public async Task<int> ExecuteNonQueryAsync(string query)
        => await PostgreSqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), query);

    public async Task<object?> ExecuteScalarAsync(string query)
        => await PostgreSqlWrapper.ExecuteScalarAsync(await GetConnectionAsync(), query);

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