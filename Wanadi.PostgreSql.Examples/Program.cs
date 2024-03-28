using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using Wanadi.Common.Extensions;
using Wanadi.PostgreSql.Examples;
using Wanadi.PostgreSql.Wrappers;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

var connectionString = PostgreSqlWrapper.BuildConnectionString("192.168.2.11", "postgres", "postgres", "wanadi");

var lista1000 = GenerateData(1000);
var lista10000 = GenerateData(10000);
var lista100000 = GenerateData(100000);
var lista1000000 = GenerateData(1000000);

var repository = new TableRepository(connectionString);

var readingTest = await repository.SelectQueryAsync<TableEntity>("SELECT * FROM table_test LIMIT 10;");

var sw = new Stopwatch();

sw.Start();

await repository.BinaryImportAsync(lista1000);

sw.Stop();

$"1.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
sw.Reset();
sw.Start();

await repository.BinaryImportAsync(lista10000);

sw.Stop();

$"10.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
sw.Reset();
sw.Start();

await repository.BinaryImportAsync(lista100000);

sw.Stop();

$"100.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
sw.Reset();
sw.Start();

await repository.BinaryImportAsync(lista1000000);

sw.Stop();

$"1.000.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();

Console.ReadKey();

List<TableEntity> GenerateData(int quantity)
{
    var lista = new List<TableEntity>();

    var birth_date = new DateTime(1992, 10, 28);

    for (int i = 0; i < quantity; i++)
    {
        try
        {
            birth_date = birth_date.AddDays(i);
            if (birth_date > DateTime.Now)
                birth_date = new DateTime(1992, 10, 28);
        }
        catch
        {
            birth_date = new DateTime(1992, 10, 28);
        }

        lista.Add(new TableEntity()
        {
            BirthDate = i % 2 == 0 ? null : birth_date,
            Identifier = Guid.NewGuid(),
            IsActive = i % 2 == 0,
            Name = $"user {i}",
            Surname = $"surname {i}",
            Wage = 12.2m * (i + 1),
            Id = i
        });
    }

    return lista;
}

[Table("table_test")]
public class TableEntity
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("identifier")]
    public Guid Identifier { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("surname")]
    public string? Surname { get; set; }

    [Column("birth_date")]
    public DateTime? BirthDate { get; set; }

    [Column("wage")]
    public decimal Wage { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}