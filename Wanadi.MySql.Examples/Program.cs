using System.Diagnostics;
using System.Globalization;
using Wanadi.Common.Extensions;
using Wanadi.MySql.Examples;
using Wanadi.MySql.Examples.Data;
using Wanadi.MySql.Examples.Data.Entities;
using Wanadi.MySql.Wrappers;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

///Test
await TestPerformanceBetweenEFAndDWAsync(1000);

//1.000 records in EF elapsed in 00:00:09.4387130
//1.000 records in DW elapsed in 00:00:01.0554050

//10.000 records in EF elapsed in 00:00:56.7174500
//10.000 records in DW elapsed in 00:00:04.3088360

//100.000 records in EF elapsed in 00:09:36.0190860
//100.000 records in DW elapsed in 00:00:52.3876710

async Task TestPerformanceBetweenEFAndDWAsync(int recordsQuantity)
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