using Npgsql;

namespace Wanadi.PostgreSql.Interfaces;

public interface IWanadiPostgreSqlRepository<TEntity> : IDisposable where TEntity : class
{
    string GetTableName();

    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);

    Task<List<TEntity>> ToListAsync();

    Task<List<TEntity>> SelectQueryAsync(string query);
    Task<List<T>> SelectQueryAsync<T>(string query) where T : class;

    Task<TEntity?> FirstOrDefaultAsync(string query);
    Task<T?> FirstOrDefaultAsync<T>(string query) where T : class;

    Task BinaryImportAsync(List<TEntity> entities);

    Task<int> ExecuteNonQueryAsync(string query);
    Task<object?> ExecuteScalarAsync(string query);

    Task ResetIdentityTable();

    Task<long> CountAsync();

    Task<NpgsqlConnection> GetConnectionAsync();
}