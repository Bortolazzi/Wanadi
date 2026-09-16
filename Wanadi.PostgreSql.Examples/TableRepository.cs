using Npgsql;
using Wanadi.PostgreSql.Repositories;

namespace Wanadi.PostgreSql.Examples;

public class TableRepository : WanadiPostgreSqlRepository<TableEntity>
{
    public TableRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }
}
