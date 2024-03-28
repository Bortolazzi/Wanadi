using Wanadi.Clickhouse.Models;

namespace Wanadi.Clickhouse.Interfaces;

public interface IClickhouseRepository<TEntity> : IDisposable where TEntity : ClickHouseEnumeratorEntity
{
	string GetTableName();

	Task<List<TEntity>> SelectQueryAsync(string query);
	Task<List<T>> SelectQueryAsync<T>(string query) where T : class;

	Task<TEntity?> FirstOrDefaultAsync(string query);
	Task<T?> FirstOrDefaultAsync<T>(string query) where T : class;

	Task BatchInsertAsync(List<TEntity> entities, int batchQuantity = 5000);
	Task BulkInsertAsync(List<TEntity> entities);

	Task<int> ExecuteNonQueryAsync(string query);
	Task<object?> ExecuteScalarAsync(string query);

	Task<long> CountAsync();
}