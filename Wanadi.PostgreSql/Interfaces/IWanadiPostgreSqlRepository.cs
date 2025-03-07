﻿using Npgsql;

namespace Wanadi.PostgreSql.Interfaces;

public interface IWanadiPostgreSqlRepository<TEntity> : IDisposable where TEntity : class
{
    string GetTableName();

    Task<TEntity?> AddAsync(TEntity entity, string? tableName = null);
    Task UpdateAsync(TEntity entity, string? tableName = null);

    Task<TEntity?> GetByIdAsync(int id);
    Task DeleteByIdAsync(int id);

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