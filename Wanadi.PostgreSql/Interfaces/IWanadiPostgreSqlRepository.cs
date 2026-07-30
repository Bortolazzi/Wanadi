using Npgsql;

namespace Wanadi.PostgreSql.Interfaces;

public interface IWanadiPostgreSqlRepository<TEntity> : IDisposable where TEntity : class
{
    string GetTableName();

    Task<TEntity?> AddAsync(TEntity entity, string? tableName = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, string? tableName = null, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> QueryAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    Task<List<T>> QueryAsync<T>(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where T : class;

    Task<TEntity?> FirstOrDefaultAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync<T>(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default) where T : class;

    Task BinaryImportAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    Task<int> ExecuteNonQueryAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    Task<object?> ExecuteScalarAsync(string query, List<NpgsqlParameter>? parameters = null, CancellationToken cancellationToken = default);

    Task TruncateTableAsync(CancellationToken cancellationToken = default);
    Task ResetIdentityTableAsync(CancellationToken cancellationToken = default);

    Task<long> CountAsync(CancellationToken cancellationToken = default);

    Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
