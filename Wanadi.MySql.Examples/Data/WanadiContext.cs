using Microsoft.EntityFrameworkCore;
using Wanadi.MySql.Examples.Data.Entities;

namespace Wanadi.MySql.Examples.Data;

public class WanadiContext : DbContext
{
	public WanadiContext(DbContextOptions<WanadiContext> options) : base(options) { }

    public virtual DbSet<TableTestEntity> TableTests { get; set; }
    public virtual DbSet<TableTestEfEntity> TableTestsEf { get; set; }
}