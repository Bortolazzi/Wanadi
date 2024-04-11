using Npgsql;

namespace Wanadi.PostgreSql.Interfaces;

public interface IWanadiPostgreSqlRepository<TEntity> : IDisposable where TEntity : class
{
    string GetTableName();

    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);

    Task<TEntity?> GetByIdAsync(int id);

    Task<List<TEntity>> ToListAsync();

    Task<List<TEntity>> SelectQueryAsync(string query, List<NpgsqlParameter>? parameters = null);
    Task<List<T>> SelectQueryAsync<T>(string query, List<NpgsqlParameter>? parameters = null) where T : class;

    Task<TEntity?> FirstOrDefaultAsync(string query, List<NpgsqlParameter>? parameters = null);
    Task<T?> FirstOrDefaultAsync<T>(string query, List<NpgsqlParameter>? parameters = null) where T : class;

    Task BinaryImportAsync(List<TEntity> entities);

    Task<int> ExecuteNonQueryAsync(string query, List<NpgsqlParameter>? parameters = null);
    Task<object?> ExecuteScalarAsync(string query, List<NpgsqlParameter>? parameters = null);

    Task TruncateTableAsync();
    Task ResetIdentityTableAsync();

    Task<long> CountAsync();

    Task<NpgsqlConnection> GetConnectionAsync();
}