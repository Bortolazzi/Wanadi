using ClickHouse.Ado;
using Wanadi.Clickhouse.Interfaces;
using Wanadi.Clickhouse.Models;
using Wanadi.Clickhouse.Wrappers;
using Wanadi.Common.Extensions;

namespace Wanadi.Clickhouse.Repositories;

public abstract class ClickhouseRepository<TEntity> : IClickhouseRepository<TEntity> where TEntity : ClickHouseEnumeratorEntity, new()
{
    private ClickHouseConnection? _connection;
    private ClickHouseConnectionSettings _settings { get; set; }

    public ClickhouseRepository(ClickHouseConnectionSettings settings)
    {
        _settings = settings;
    }

    public string GetTableName()
        => typeof(TEntity).GetTableName();

    public async Task<List<TEntity>> SelectQueryAsync(string query)
        => await ClickHouseWrapper.SelectQueryAsync<TEntity>(await GetConnectionAsync(), query);

    public async Task<List<T>> SelectQueryAsync<T>(string query) where T : class
        => await ClickHouseWrapper.SelectQueryAsync<T>(await GetConnectionAsync(), query);

    public async Task<TEntity?> FirstOrDefaultAsync(string query)
        => await ClickHouseWrapper.SelectQueryFirstOrDefaultAsync<TEntity>(await GetConnectionAsync(), query);

    public async Task<T?> FirstOrDefaultAsync<T>(string query) where T : class
        => await ClickHouseWrapper.SelectQueryFirstOrDefaultAsync<T>(await GetConnectionAsync(), query);

    public async Task BatchInsertAsync(List<TEntity> entities, int batchQuantity = 5000)
        => await ClickHouseWrapper.BatchInsertAsync(await GetConnectionAsync(), entities, batchQuantity);

    public async Task BulkInsertAsync(List<TEntity> entities)
        => await ClickHouseWrapper.BulkInsertAsync(_settings, entities);

    public async Task<int> ExecuteNonQueryAsync(string query)
        => await ClickHouseWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), query);

    public async Task<object?> ExecuteScalarAsync(string query)
        => await ClickHouseWrapper.ExecuteScalarAsync(await GetConnectionAsync(), query);

    public async Task<long> CountAsync()
    {
        var response = await ExecuteScalarAsync($"SELECT COUNT(1) FROM {GetTableName()};");
        if (response == null)
            return 0;

        return Convert.ToInt64(response);
    }

    private async Task<ClickHouseConnection> GetConnectionAsync()
    {
        if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            return _connection;

        _connection = await ClickHouseWrapper.GetConnectionAsync(_settings);
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}