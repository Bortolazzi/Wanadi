using Wanadi.PostgreSql.Repositories;

namespace Wanadi.PostgreSql.Examples;

public class TableRepository : WanadiPostgreSqlRepository<TableEntity>
{
    public TableRepository(string connectionString) : base(connectionString)
    {
    }
}

