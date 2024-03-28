using ClickHouse.Ado;
using Wanadi.Clickhouse.Repositories;

namespace Wanadi.Clickhouse.Examples;

public class TableTestRepository : ClickhouseRepository<TableTestEntity>
{
    public TableTestRepository(ClickHouseConnectionSettings settings) : base(settings) { }
}