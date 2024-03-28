using MySql.Data.MySqlClient;
using Wanadi.Common.Extensions;
using Wanadi.MySql.Interfaces;
using Wanadi.MySql.Models;
using Wanadi.MySql.Wrappers;

namespace Wanadi.MySql.Repositories;

public abstract class WanadiMySqlRepository<TEntity> : IWanadiMySqlRepository<TEntity> where TEntity : MySqlEntity, new()
{
    private MySqlConnection? _connection { get; set; }
    readonly string _connectionString;

    public WanadiMySqlRepository(string connectionString)
        => _connectionString = connectionString;

    public string GetTableName()
        => typeof(TEntity).GetTableName();

    public async Task AddAsync(TEntity entity)
    {
        var insertCommand = DataWrapper.GenerateInsertCommand(entity);

        if (insertCommand is null)
            return;

        await ExecuteNonQueryAsync(insertCommand.MySqlCommand);
    }

    public async Task<List<TEntity>> ToListAsync()
        => await MySqlWrapper.SelectQueryByEntityAsync<TEntity>(await GetConnectionAsync());

    public async Task<List<TEntity>> SelectQueryAsync(string query)
        => await MySqlWrapper.SelectQueryAsync<TEntity>(await GetConnectionAsync(), query);

    public async Task<List<T>> SelectQueryAsync<T>(string query) where T : class
        => await MySqlWrapper.SelectQueryAsync<T>(await GetConnectionAsync(), query);

    public async Task<TEntity?> FirstOrDefaultAsync(string query)
        => await MySqlWrapper.SelectQueryFirstOrDefaultAsync<TEntity>(await GetConnectionAsync(), query);

    public async Task<T?> FirstOrDefaultAsync<T>(string query) where T : class
        => await MySqlWrapper.SelectQueryFirstOrDefaultAsync<T>(await GetConnectionAsync(), query);

    public async Task BatchInsertAsync(List<TEntity> entities, int batchQuantity = 5000)
    {
        var batches = DataWrapper.GenerateBatchCommands(entities, batchQuantity);

        await MySqlWrapper.BatchInsertAsync(await GetConnectionAsync(), batches);
    }

    public async Task BulkInsertAsync(List<TEntity> entities)
        => await MySqlWrapper.BulkInsertAsync(_connectionString, entities);

    public async Task<int> ExecuteNonQueryAsync(string query)
        => await MySqlWrapper.ExecuteNonQueryAsync(await GetConnectionAsync(), query);

    public async Task<object?> ExecuteScalarAsync(string query)
        => await MySqlWrapper.ExecuteScalarAsync(await GetConnectionAsync(), query);

    public async Task<long> CountAsync()
    {
        var response = await ExecuteScalarAsync($"SELECT COUNT(1) FROM {GetTableName()};");
        if (response == null)
            return 0;

        return Convert.ToInt64(response);
    }

    private async Task<MySqlConnection> GetConnectionAsync()
    {
        if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            return _connection;

        _connection = await MySqlWrapper.GetConnectionAsync(_connectionString);
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}