using System.Globalization;
using ClickHouse.Ado;
using Wanadi.Clickhouse.Examples;
using Wanadi.Clickhouse.Wrappers;
using Wanadi.Common.Extensions;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

var settings = ClickHouseWrapper.BuildConnectionSettings("192.168.60.35", "wanadi", "default", null, socketTimeout: 0);

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
            var start = DateTime.Now;

            await ExecuteBatchInsertAsync(connection, source1000);

            $"Time elapsed to insert 1000 rows {DateTime.Now.Subtract(start)}".PrintInfo();
            start = DateTime.Now;

            await ExecuteBatchInsertAsync(connection, source10000);

            $"Time elapsed to insert 10000 rows {DateTime.Now.Subtract(start)}".PrintInfo();
            start = DateTime.Now;

            await ExecuteBatchInsertAsync(connection, source100000);

            $"Time elapsed to insert 100000 rows {DateTime.Now.Subtract(start)}".PrintInfo();
            start = DateTime.Now;

            await ExecuteBatchInsertAsync(connection, source1000000);

            $"Time elapsed to insert 1000000 rows {DateTime.Now.Subtract(start)}".PrintInfo();
        }

        //ClickHouse - Time elapsed to insert 1000 rows 00:00:00.9968200
        //MySqlServer -  Time elapsed to insert 1000 rows 00:00:03.1916450

        //ClickHouse - Time elapsed to insert 10000 rows 00:00:02.8870980
        //MySqlServer - Time elapsed to insert 10000 rows 00:00:03.2677990

        //ClickHouse - Time elapsed to insert 100000 rows 00:00:19.1549280
        //MySqlServer - Time elapsed to insert 100000 rows 00:00:16.8131860

        //ClickHouse - Time elapsed to insert 1000000 rows 00:03:04.5414290
        //MySqlServer - Time elapsed to insert 1000000 rows 00:02:44.6377380
    }
    catch (Exception ex)
    {
        ex.Message.PrintError(ex);
    }
}

async Task ExecuteBatchInsertAsync(ClickHouseConnection connection, List<TableTestEntity> sourceItems)
{
    await ClickHouseWrapper.BatchInsertAsync(connection, sourceItems);
}

List<TableTestEntity> GenerateDataToInsert(int quantity)
{
    var response = new List<TableTestEntity>();

    var birthDate = new DateTime(1992, 10, 28);

    for (int i = 0; i < quantity; i++)
    {
        response.Add(new TableTestEntity()
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