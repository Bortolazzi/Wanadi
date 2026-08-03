using System.Globalization;
using System.Collections;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Wanadi.Common.Extensions;
using Wanadi.Common.Helpers;
using Wanadi.Common.Wrappers;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DebugExamples");
Directory.CreateDirectory(outputDirectory);

RunIListExtensionsExamples();
await RunExcelWrapperListExample(outputDirectory);
await RunExcelWrapperDataTableExample(outputDirectory);
await RunExcelWrapperJsonExamples(outputDirectory);

Console.WriteLine();
Console.WriteLine($"Arquivos gerados em: {outputDirectory}");

static void RunIListExtensionsExamples()
{
    var genericList = new List<ExcelDebugPerson>();
    IList genericAsIList = genericList;
    IList nonGenericList = new ArrayList();
    var anotherList = new List<object>().Select(t => new { Name = string.Empty }).ToList();

    Console.WriteLine("[IListExtensions]");
    Console.WriteLine($"IList generico.GetTableName(): {genericAsIList.GetTableName()}");
    Console.WriteLine($"ArrayList.GetTableName(): {nonGenericList.GetTableName() ?? "null"}");
}

static async Task RunExcelWrapperListExample(string outputDirectory)
{
    var people = new List<ExcelDebugPerson>
    {
        new ExcelDebugPerson("Renato", 35, 1234.56m, new DateTime(2026, 7, 30)),
        new ExcelDebugPerson("Ana", 29, 98765.43m, new DateTime(2026, 8, 1))
    };

    var fileName = Path.Combine(outputDirectory, "list-export.xlsx");
    if (File.Exists(fileName))
        File.Delete(fileName);

    await people.ExportToExcelAsync(fileName, "Pessoas", reviewStyleCell: true);

    Console.WriteLine("[ExcelWrapper - List]");
    Console.WriteLine(fileName);
}

static async Task RunExcelWrapperDataTableExample(string outputDirectory)
{
    var table = new DataTable();
    table.Columns.Add("Nome", typeof(string));
    table.Columns.Add("Idade", typeof(int));
    table.Columns.Add("Saldo", typeof(decimal));
    table.Columns.Add("CriadoEm", typeof(DateTime));

    table.Rows.Add("Cliente A", 41, 120.50m, new DateTime(2026, 7, 30));
    table.Rows.Add("Cliente B", 52, 999.99m, new DateTime(2026, 8, 15));

    var fileName = Path.Combine(outputDirectory, "datatable-export.xlsx");
    if (File.Exists(fileName))
        File.Delete(fileName);

    await table.ExportToExcelAsync(fileName, "Clientes", reviewStyleCell: true);

    Console.WriteLine("[ExcelWrapper - DataTable]");
    Console.WriteLine(fileName);
}

static async Task RunExcelWrapperJsonExamples(string outputDirectory)
{
    var json = """
    [
        {
            "Nome": "Pedido A",
            "Quantidade": 3,
            "Valor": 99.90,
            "CriadoEm": "2026-07-30T10:15:00",
            "Cliente": {
                "Nome": "Renato",
                "Documento": "12345678000199"
            },
            "Tags": ["novo", "prioritario"]
        },
        {
            "Nome": "Pedido B",
            "Quantidade": 12,
            "Valor": 1500.75,
            "CriadoEm": "2026-08-01T08:30:00",
            "Cliente": {
                "Nome": "Ana",
                "Documento": "98765432000188"
            },
            "Tags": ["recorrente"]
        }
    ]
    """;

    var flattenedDataTable = JsonHelper.JsonArrayToDataTable(json, flattenNestedObjects: true);
    Console.WriteLine("[JsonHelper]");
    Console.WriteLine($"Colunas com flatten: {string.Join(", ", flattenedDataTable.Columns.Cast<DataColumn>().Select(t => t.ColumnName))}");

    var flattenFileName = Path.Combine(outputDirectory, "json-flatten-export.xlsx");
    if (File.Exists(flattenFileName))
        File.Delete(flattenFileName);

    await ExcelWrapper.JsonToFileAsync(flattenFileName, json, "JsonFlatten", reviewStyleCell: true, flattenNestedObjects: true);

    var compactFileName = Path.Combine(outputDirectory, "json-compact-export.xlsx");
    if (File.Exists(compactFileName))
        File.Delete(compactFileName);

    await json.ExportJsonToExcelAsync(compactFileName, "JsonCompact", reviewStyleCell: true, flattenNestedObjects: false);

    var jsonFilePath = Path.Combine(outputDirectory, "source.json");
    if (File.Exists(jsonFilePath))
        File.Delete(jsonFilePath);

    await File.WriteAllTextAsync(jsonFilePath, json);

    var fromFileName = Path.Combine(outputDirectory, "json-file-export.xlsx");
    if (File.Exists(fromFileName))
        File.Delete(fromFileName);

    await ExcelWrapper.JsonFileToFileAsync(fromFileName, jsonFilePath, "JsonFile", reviewStyleCell: true, flattenNestedObjects: true);

    Console.WriteLine("[ExcelWrapper - JSON]");
    Console.WriteLine(flattenFileName);
    Console.WriteLine(compactFileName);
    Console.WriteLine(fromFileName);
}

[Table("debug_people")]
public sealed record ExcelDebugPerson(string Nome, int Idade, decimal Saldo, DateTime CriadoEm);
