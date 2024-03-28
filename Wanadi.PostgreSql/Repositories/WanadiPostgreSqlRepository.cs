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

    public async Task<long> CountAsync()
    {
        var response = await ExecuteScalarAsync($"SELECT COUNT(1) FROM {GetTableName()};");
        if (response == null)
            return 0;

        return Convert.ToInt64(response);
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
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