using System.Diagnostics;
using System.Globalization;
using Wanadi.Clickhouse.Examples;
using Wanadi.Clickhouse.Wrappers;
using Wanadi.Common.Extensions;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

var settings = ClickHouseWrapper.BuildConnectionSettings("192.168.60.35", "wanadi", "default", null, socketTimeout: 0);

await BulkInsertTestAsync();
await BatchInsertTestAsync();

Console.ReadKey();

async Task BatchInsertTestAsync()
{
    try
    {
        var source1000 = GenerateDataToInsert(1000);
        var source10000 = GenerateDataToInsert(10000);
        var source100000 = GenerateDataToInsert(100000);
        var source1000000 = GenerateDataToInsert(1000000);

        using (var connection = await ClickHouseWrapper.GetConnectionAsync(settings))
        {
            var sw = new Stopwatch();

            sw.Start();

            await ClickHouseWrapper.BatchInsertAsync(connection, source1000);

            sw.Stop();

            $"1.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
            sw.Reset();
            sw.Start();

            await ClickHouseWrapper.BatchInsertAsync(connection, source10000);

            sw.Stop();

            $"10.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
            sw.Reset();
            sw.Start();

            await ClickHouseWrapper.BatchInsertAsync(connection, source100000);

            sw.Stop();

            $"100.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
            sw.Reset();
            sw.Start();

            await ClickHouseWrapper.BatchInsertAsync(connection, source1000000);

            sw.Stop();

            $"1.000.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
        }

        //ClickHouse  - Batch -     1.000 rows insert elapsed in 00:00:00.9968200
        //MySqlServer - Batch -     1.000 rows insert elapsed in 00:00:03.1916450

        //ClickHouse  - Batch -    10.000 rows insert elapsed in 00:00:02.8870980
        //MySqlServer - Batch -    10.000 rows insert elapsed in 00:00:03.2677990

        //ClickHouse  - Batch -   100.000 rows insert elapsed in 00:00:19.1549280
        //MySqlServer - Batch -   100.000 rows insert elapsed in 00:00:16.8131860

        //ClickHouse  - Batch - 1.000.000 rows insert elapsed in 00:03:04.5414290
        //MySqlServer - Batch - 1.000.000 rows insert elapsed in 00:02:44.6377380
    }
    catch (Exception ex)
    {
        ex.Message.PrintError(ex);
    }
}

async Task BulkInsertTestAsync()
{
    try
    {
        var source1000 = GenerateDataToInsert(1000);
        var source10000 = GenerateDataToInsert(10000);
        var source100000 = GenerateDataToInsert(100000);
        var source1000000 = GenerateDataToInsert(1000000);

        var sw = new Stopwatch();

        sw.Start();

        await ClickHouseWrapper.BulkInsertAsync(settings, source1000);

        sw.Stop();

        $"1.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
        sw.Reset();
        sw.Start();

        await ClickHouseWrapper.BulkInsertAsync(settings, source10000);

        sw.Stop();

        $"10.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
        sw.Reset();
        sw.Start();

        await ClickHouseWrapper.BulkInsertAsync(settings, source100000);

        sw.Stop();

        $"100.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();
        sw.Reset();
        sw.Start();

        await ClickHouseWrapper.BulkInsertAsync(settings, source1000000);

        sw.Stop();

        $"1.000.000 rows insert elapsed in {sw.Elapsed}".PrintInfo();

        //ClickHouse  - Batch -     1.000 rows insert elapsed in 00:00:00.9968200
        //MySqlServer - Batch -     1.000 rows insert elapsed in 00:00:03.1916450

        //ClickHouse  -  Bulk -     1.000 rows insert elapsed in 00:00:01.3134535
        //MySqlServer -  Bulk -     1.000 rows insert elapsed in 00:00:02.1574839

        //ClickHouse  - Batch -    10.000 rows insert elapsed in 00:00:02.8870980
        //MySqlServer - Batch -    10.000 rows insert elapsed in 00:00:03.2677990

        //ClickHouse  -  Bulk -    10.000 rows insert elapsed in 00:00:01.3680546
        //MySqlServer -  Bulk -    10.000 rows insert elapsed in 00:00:02.7510941

        //ClickHouse  - Batch -   100.000 rows insert elapsed in 00:00:19.1549280
        //MySqlServer - Batch -   100.000 rows insert elapsed in 00:00:16.8131860

        //ClickHouse  -  Bulk -   100.000 rows insert elapsed in 00:00:08.9147645
        //MySqlServer -  Bulk -   100.000 rows insert elapsed in 00:00:11.2027401

        //ClickHouse  - Batch - 1.000.000 rows insert elapsed in 00:03:04.5414290
        //MySqlServer - Batch - 1.000.000 rows insert elapsed in 00:02:44.6377380

        //ClickHouse  -  Bulk - 1.000.000 rows insert elapsed in 00:00:24.4038803
        //MySqlServer -  Bulk - 1.000.000 rows insert elapsed in 00:01:38.3122330
    }
    catch (Exception ex)
    {
        ex.Message.PrintError(ex);
    }
}

List<TableTestEntity> GenerateDataToInsert(int quantity)
{
    var response = new List<TableTestEntity>();

    var birthDate = new DateTime(1992, 10, 28);

    for (int i = 0; i < quantity; i++)
    {
        birthDate = birthDate.AddDays(i);
        if (birthDate >= DateTime.Now)
            birthDate = new DateTime(1992, 10, 28);

        response.Add(new TableTestEntity()
        {
            Id = Guid.NewGuid(),
            Name = $"test {i}",
            BirthDate = (i % 2) == 0 ? birthDate : null,
            Age = i,
            IsActive = true,
            Wage = decimal.Round(1.12m * i, 2, MidpointRounding.AwayFromZero),
            CreatedAt = DateTime.MinValue
        });
    }

    return response;
}