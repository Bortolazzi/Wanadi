using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wanadi.Common.Attributes;

namespace Wanadi.MySql.Examples.Data.Entities;

[Table("table_test_ef")]
public class TableTestEfEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Id")]
    public int Id { get; set; }

    [Column("Description"), StringLength(255), Required]
    public string RecordDescription { get; set; }

    [Column("Complement"), StringLength(255)]
    public string? ComplementDescription { get; set; }

    [Column("EnumStatus")]
    public int Status { get; set; }

    [Column("UUID")]
    public string Uuid { get; set; }

    [Column("DateEnd"), DataType(DataType.Date)]
    public DateTime? DateEnd { get; set; }

    [Column("Price"), DecimalPrecision(18, 2)]
    public decimal RecordPrice { get; set; }

    [Column("CreatedAt"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

    [Column("Active")]
    public bool IsActive { get; set; }
}