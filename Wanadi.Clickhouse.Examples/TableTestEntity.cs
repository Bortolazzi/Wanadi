using System.ComponentModel.DataAnnotations.Schema;
using Wanadi.Clickhouse.Models;
using Wanadi.Common.Attributes;

namespace Wanadi.Clickhouse.Examples;

[Table("wanadi_test"), Database("wanadi")]
public class TableTestEntity : ClickHouseEnumeratorEntity
{
    public TableTestEntity() { }

    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("birth_date")]
    public DateTime? BirthDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("wage")]
    public decimal Wage { get; set; }

    [Column("age")]
    public int Age { get; set; }

    [Column("created_at"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }
}