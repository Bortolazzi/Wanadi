using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using Wanadi.Common.Extensions;
using Wanadi.MySql.Examples;
using Wanadi.MySql.Examples.Data;
using Wanadi.MySql.Examples.Data.Entities;
using Wanadi.MySql.Wrappers;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");


/// pt-BR: Método para inserção e comparação de perfomance entre o EntityFramework Core (MySql) e o DataWrapper.
/// en-US: Method for insertion and performance comparison between Entity Framework Core (MySql) and DataWrapper.
await TestInsertPerformanceBetweenEFAndDWAsync(1000);

/// pt-BR: Método para inserção e comparação de perfomance entre o EntityFramework Core (MySql) e o DataWrapper.
/// en-US: Method for insertion and performance comparison between Entity Framework Core (MySql) and DataWrapper.
async Task TestInsertPerformanceBetweenEFAndDWAsync(int recordsQuantity)
{
    var sw = new Stopwatch();

    sw.Start();

    $"Inserting data with EF".PrintInfo();
    await GenerateTableTestRecordsToTestUsingEFAsync(recordsQuantity);

    sw.Stop();

    $"Insert with EF elapsed in {sw.Elapsed}".PrintWarning();

    sw.Reset();
    sw.Start();

    $"Inserting data with DW".PrintInfo();
    await GenerateTableTestRecordsToTestUsingDataWrapperAsync(recordsQuantity);

    sw.Stop();
    $"Insert with DW elapsed in {sw.Elapsed}".PrintWarning();
    Console.ReadKey();

    //1.000 records in EF elapsed in 00:00:09.4387130
    //1.000 records in DW elapsed in 00:00:01.0554050

    //10.000 records in EF elapsed in 00:00:56.7174500
    //10.000 records in DW elapsed in 00:00:04.3088360

    //100.000 records in EF elapsed in 00:09:36.0190860
    //100.000 records in DW elapsed in 00:00:52.3876710

}

async Task TesteInsertPerfomanceBulkInsertMySqlAsync()
{
    var source1000 = GenerateDataToInsert(1000);
    var source10000 = GenerateDataToInsert(10000);
    var source100000 = GenerateDataToInsert(100000);
    var source1000000 = GenerateDataToInsert(1000000);

    DataWrapper.GuidOption = Wanadi.Common.Enums.GuidConditions.CastToString;

    var sw = new Stopwatch();

    sw.Start();

    await MySqlWrapper.ExecuteBulkInsertAsync(ContextWrapper.ConnectionString, source1000);

    sw.Stop();

    $"1.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
    sw.Reset();
    sw.Start();

    await MySqlWrapper.ExecuteBulkInsertAsync(ContextWrapper.ConnectionString, source10000);

    sw.Stop();

    $"10.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
    sw.Reset();
    sw.Start();

    await MySqlWrapper.ExecuteBulkInsertAsync(ContextWrapper.ConnectionString, source100000);

    sw.Stop();

    $"100.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
    sw.Reset();
    sw.Start();

    await MySqlWrapper.ExecuteBulkInsertAsync(ContextWrapper.ConnectionString, source1000000);

    sw.Stop();

    $"1.000.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();

    //ClickHouse  - Batch -     1.000 rows insert elapsed in 00:00:00.9968200
    //MySqlServer - Batch -     1.000 rows insert elapsed in 00:00:03.1916450

    //MySqlServer -  Bulk -     1.000 rows insert elapsed in 00:00:02.1574839

    //ClickHouse  - Batch -    10.000 rows insert elapsed in 00:00:02.8870980
    //MySqlServer - Batch -    10.000 rows insert elapsed in 00:00:03.2677990

    //MySqlServer -  Bulk -    10.000 rows insert elapsed in 00:00:02.7510941

    //ClickHouse  - Batch -   100.000 rows insert elapsed in 00:00:19.1549280
    //MySqlServer - Batch -   100.000 rows insert elapsed in 00:00:16.8131860

    //MySqlServer -  Bulk -   100.000 rows insert elapsed in 00:00:11.2027401

    //ClickHouse  - Batch - 1.000.000 rows insert elapsed in 00:03:04.5414290
    //MySqlServer - Batch - 1.000.000 rows insert elapsed in 00:02:44.6377380

    //MySqlServer -  Bulk - 1.000.000 rows insert elapsed in 00:01:38.3122330


    Console.ReadKey();
}

async Task GenerateTableTestRecordsToTestUsingDataWrapperAsync(int recordsQuantity)
{
    var sourceInsert = new List<TableTestEntity>();
    
    for (int i = 0; i < recordsQuantity; i++)
    {
        sourceInsert.Add(new TableTestEntity()
        {
            ComplementDescription = DataGenerator.RandomString(DataGenerator.RandomInt(0, 255)),
            CreatedAt = DateTime.Now,
            DateEnd = DataGenerator.RandomDateMonth(DataGenerator.RandomInt(0, 12)),
            IsActive = true,
            RecordDescription = DataGenerator.RandomString(DataGenerator.RandomInt(1, 255)),
            RecordPrice = DataGenerator.RandomDecimal(2, 6),
            Status = DataGenerator.RandomEnumStatus(),
            UpdatedAt = DateTime.Now,
            Uuid = Guid.NewGuid(),
        });
    }

    DataWrapper.GuidOption = Wanadi.Common.Enums.GuidConditions.CastToString;
    DataWrapper.EnumOption = Wanadi.Common.Enums.EnumConditions.CastToInt;

    var batches = DataWrapper.GenerateBatchCommandsParallel(sourceInsert, 5000, false);
    int counter = 0;
    foreach (var batch in batches)
    {
        counter++;
        $"Executing batch command {counter:N0} of {batches.Count:N0}".PrintInfo();

        await MySqlWrapper.ExecuteNonQueryAsync(ContextWrapper.ConnectionString, batch.MySqlCommand);
    }
}

async Task GenerateTableTestRecordsToTestUsingEFAsync(int recordsQuantity)
{
    var sourceInsert = new List<TableTestEfEntity>();

    for (int i = 0; i < recordsQuantity; i++)
    {
        sourceInsert.Add(new TableTestEfEntity()
        {
            ComplementDescription = DataGenerator.RandomString(DataGenerator.RandomInt(0, 255)),
            CreatedAt = DateTime.Now,
            DateEnd = DataGenerator.RandomDateMonth(DataGenerator.RandomInt(0, 12)),
            IsActive = true,
            RecordDescription = DataGenerator.RandomString(DataGenerator.RandomInt(1, 255)),
            RecordPrice = DataGenerator.RandomDecimal(2, 2),
            Status = (int)DataGenerator.RandomEnumStatus(),
            UpdatedAt = DateTime.Now,
            Uuid = Guid.NewGuid().ToString(),
        });
    }

    var context = ContextWrapper.Wanadi();

    await context.AddRangeAsync(sourceInsert);
    await context.SaveChangesAsync();
}

List<TableTestClEntity> GenerateDataToInsert(int quantity)
{
    var response = new List<TableTestClEntity>();

    var birthDate = new DateTime(1992, 10, 28);

    for (int i = 0; i < quantity; i++)
    {
        response.Add(new TableTestClEntity()
        {
            Id = Guid.NewGuid(),
            Name = $"test {i}",
            BirthDate = (i % 2) == 0 ? birthDate.AddDays(i) : null,
            Age = i,
            IsActive = true,
            Wage = decimal.Round(1.12m * i, 2, MidpointRounding.AwayFromZero),
            CreatedAt = DateTime.MinValue
        });
    }

    return response;
}

[Table("wanadi_test")]
public class TableTestClEntity 
{
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