using System.Data;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Wanadi.Common.Attributes;
using Wanadi.Common.Contracts.Wrappers.Excel;

namespace Wanadi.Common.Wrappers;

public static class ExcelWrapper
{
    public static Color HeaderBackgroundColor { get; set; } = Color.Purple;
    public static Color HeaderFontColor { get; set; } = Color.White;
    public static string FontName { get; set; } = "Courier New";
    public static float FontSize { get; set; } = 12;


    private static string GetCellLetter(string addressMaxColunm, int row = 1)
    {
        string celulaFinalCabecalho = addressMaxColunm.Split(':')[1];
        return celulaFinalCabecalho.Replace(celulaFinalCabecalho.RemoveNotNumeric(), row.ToString());
    }

    private static List<ExcelWrapperWorksheetColumn> ReadHeadersFromWorksheet(ExcelWorksheet worksheet, ExcelCellAddress start, ExcelCellAddress end)
    {
        var response = new List<ExcelWrapperWorksheetColumn>();

        for (int currentCell = start.Column; currentCell <= end.Column; currentCell++)
        {
            object cellValue = worksheet.Cells[start.Row, currentCell].Value;

            if (cellValue == null)
                continue;

            response.Add(new ExcelWrapperWorksheetColumn(currentCell, cellValue));
        }

        return response;
    }

    private static List<ExcelWrapperPropertyMapping> MappingColumnsToProperty<T>(List<ExcelWrapperWorksheetColumn> headersExcel)
    {
        var response = new List<ExcelWrapperPropertyMapping>();

        var typeProperties = typeof(T).GetProperties()
                                      .Select(t => new
                                      {
                                          Property = t,
                                          PropertyName = t.Name.ToLower().RemoveAccents().Replace("\n", " ").Replace(" ", "_").Trim(),
                                          HeaderAttribute = t.GetAttribute<ExcelHeaderAttribute>()
                                      }).ToList();

        foreach (var property in typeProperties)
        {
            if (property.HeaderAttribute is not null)
            {
                foreach (var header in headersExcel)
                {
                    if (property.HeaderAttribute.ColumnNames.Contains(header.ColumnName))
                    {
                        response.Add(new ExcelWrapperPropertyMapping(property.Property, property.PropertyName, header.ColumnIndex, property.HeaderAttribute.TypeToConvert));
                        break;
                    }
                }

                continue;
            }

            foreach (var header in headersExcel)
            {
                if (property.PropertyName == header.ColumnName)
                {
                    response.Add(new ExcelWrapperPropertyMapping(property.Property, property.PropertyName, header.ColumnIndex, property.Property.PropertyType));
                    break;
                }
            }
        }

        return response;
    }

    private static void FormatStyleColumns<T>(ExcelWorksheet worksheet)
    {
        if (worksheet.Dimension == null)
            return;

        var properties = typeof(T).GetProperties()
                                  .ToDictionary(t => t.Name, t => t.PropertyType, StringComparer.OrdinalIgnoreCase);

        FormatStyleColumns(worksheet, properties);
    }

    private static void FormatStyleColumns(ExcelWorksheet worksheet, DataTable sourceData)
    {
        if (worksheet.Dimension == null)
            return;

        var columns = sourceData.Columns
                                .Cast<DataColumn>()
                                .ToDictionary(t => t.ColumnName, t => t.DataType, StringComparer.OrdinalIgnoreCase);

        FormatStyleColumns(worksheet, columns);
    }

    private static void FormatStyleColumns(ExcelWorksheet worksheet, Dictionary<string, Type> columns)
    {
        var headerRow = worksheet.Dimension.Start.Row;
        var startColumn = worksheet.Dimension.Start.Column;
        var endColumn = worksheet.Dimension.End.Column;

        for (int col = startColumn; col <= endColumn; col++)
        {
            var header = worksheet.Cells[headerRow, col].Text;
            if (string.IsNullOrEmpty(header) || !columns.TryGetValue(header, out var columnType))
                continue;

            ApplyNumberFormat(worksheet, col, columnType);
        }
    }

    private static void ApplyNumberFormat(ExcelWorksheet worksheet, int column, Type columnType)
    {
        var type = Nullable.GetUnderlyingType(columnType) ?? columnType;

        if (type == typeof(DateTime))
            worksheet.Column(column).Style.Numberformat.Format = "dd/mm/yyyy";
        else if (type == typeof(decimal))
            worksheet.Column(column).Style.Numberformat.Format = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-";
        else if (type == typeof(int))
            worksheet.Column(column).Style.Numberformat.Format = "0";
    }

    private static void FormatHeader(ExcelWorksheet worksheet, string firstCell, string lastCell)
    {
        worksheet.Cells[$"{firstCell}:{lastCell}"].Style.Font.Bold = true;
        worksheet.Cells[$"{firstCell}:{lastCell}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[$"{firstCell}:{lastCell}"].Style.Fill.BackgroundColor.SetColor(ExcelWrapper.HeaderBackgroundColor);
        worksheet.Cells[$"{firstCell}:{lastCell}"].Style.Font.Color.SetColor(ExcelWrapper.HeaderFontColor);
    }

    private static void ApplyNicknames(ExcelWorksheet worksheet, List<ExcelWrapperColumnNickname> columnsNickname)
    {
        if (columnsNickname == null || columnsNickname.Count == 0)
            return;

        ExcelRange row = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];

        foreach (ExcelRangeBase cell in row)
        {
            var nickName = columnsNickname.FirstOrDefault(p => p.ColumnName.Replace("_", " ") == cell.Text.Replace("_", " "));
            if (nickName != null)
            {
                worksheet.Cells[cell.Address].Value = nickName.ColumnNickname;
            }
        }
    }

    public static List<T>? ReadToList<T>(string pathFile, string? worksheetName = null) where T : class
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(pathFile)))
        {
            if (excelPackage.Workbook.Worksheets.Count == 0 ||
                (!string.IsNullOrEmpty(worksheetName) && !excelPackage.Workbook.Worksheets.Any(t => t.Name == worksheetName)))
                return null;

            ExcelWorksheet? worksheet = null;
            if (string.IsNullOrEmpty(worksheetName))
                worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
            else
                worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault(t => t.Name == worksheetName);

            if (worksheet.Dimension == null)
                return null;

            var start = worksheet.Dimension.Start;
            var startRowValues = start.Row + 1;
            var end = worksheet.Dimension.End;

            var headersExcel = ReadHeadersFromWorksheet(worksheet, start, end);
            var properties = MappingColumnsToProperty<T>(headersExcel);
            if (properties.Count == 0)
                return null;

            var response = new List<T>();
            for (int currentRow = startRowValues; currentRow <= end.Row; currentRow++)
            {
                T rowValues = Activator.CreateInstance<T>();
                var anyFieldFound = false;

                foreach (var property in properties)
                {
                    var cellValue = worksheet.Cells[currentRow, property.ColumnIndex].Value;
                    if (cellValue == null)
                        continue;

                    try
                    {
                        anyFieldFound = true;
                        var excelValueType = cellValue.GetType();

                        if (excelValueType == typeof(DateTime) && property.TypeToConvert == typeof(string))
                        {
                            property.Property.SetValue(rowValues, Convert.ToDateTime(cellValue).ToShortDateString());
                            continue;
                        }

                        if (property.TypeToConvert == typeof(decimal))
                            property.Property.SetValue(rowValues, ConvertHelper.ToDecimal(cellValue.ToString(), true));
                        else
                            property.Property.SetValue(rowValues, Convert.ChangeType(cellValue, property.TypeToConvert));
                    }
                    catch
                    {
                        throw new Exception($"Erro ao converter valor {cellValue} para o tipo {property.PropertyName}.");
                    }
                }

                if (anyFieldFound)
                    response.Add(rowValues);
            }

            return response;
        }
    }

    public static async Task ListToFileAsync<T>(string fileName, List<T> sourceData, string worksheetName, bool reviewStyleCell)
        => await ListToFileAsync<T>(fileName, sourceData, worksheetName, reviewStyleCell, null);

    public static async Task ListToFileAsync<T>(string fileName, List<T> sourceData, string worksheetName, bool reviewStyleCell, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
    {
        FileInfo fileInfo = new FileInfo(fileName);
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(worksheetName);
            worksheet.Cells["A1"].LoadFromCollection(sourceData, true);

            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells.Style.Fill.BackgroundColor.SetColor(Color.White);
            worksheet.Cells.Style.Font.Name = ExcelWrapper.FontName;
            worksheet.Cells.Style.Font.Size = ExcelWrapper.FontSize;

            if (columnsRemove != null && columnsRemove.Length > 0)
            {
                foreach (var columnDelete in columnsRemove.OrderByDescending(t => t))
                    worksheet.DeleteColumn(columnDelete);
            }

            string lastCell = ExcelWrapper.GetCellLetter(worksheet.Dimension.Address, 1);

            if (reviewStyleCell)
                ExcelWrapper.FormatStyleColumns<T>(worksheet);

            ExcelWrapper.FormatHeader(worksheet, "A1", lastCell);
            ExcelWrapper.ApplyNicknames(worksheet, columnsNickname);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            await excelPackage.SaveAsync();
        }
    }

    public static async Task DataTableToFileAsync(string fileName, DataTable sourceData, string worksheetName, bool reviewStyleCell)
        => await DataTableToFileAsync(fileName, sourceData, worksheetName, reviewStyleCell, null);

    public static async Task DataTableToFileAsync(string fileName, DataTable sourceData, string worksheetName, bool reviewStyleCell, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
    {
        FileInfo fileInfo = new FileInfo(fileName);
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add(worksheetName);
            worksheet.Cells["A1"].LoadFromDataTable(sourceData, true);

            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells.Style.Fill.BackgroundColor.SetColor(Color.White);
            worksheet.Cells.Style.Font.Name = ExcelWrapper.FontName;
            worksheet.Cells.Style.Font.Size = ExcelWrapper.FontSize;

            if (columnsRemove != null && columnsRemove.Length > 0)
            {
                foreach (var columnDelete in columnsRemove.OrderByDescending(t => t))
                    worksheet.DeleteColumn(columnDelete);
            }

            string lastCell = ExcelWrapper.GetCellLetter(worksheet.Dimension.Address, 1);

            if (reviewStyleCell)
                ExcelWrapper.FormatStyleColumns(worksheet, sourceData);

            ExcelWrapper.FormatHeader(worksheet, "A1", lastCell);
            ExcelWrapper.ApplyNicknames(worksheet, columnsNickname);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            await excelPackage.SaveAsync();
        }
    }

    public static async Task JsonToFileAsync(string fileName, string json, string worksheetName, bool reviewStyleCell, bool flattenNestedObjects = true)
        => await JsonToFileAsync(fileName, json, worksheetName, reviewStyleCell, flattenNestedObjects, null);

    public static async Task JsonToFileAsync(string fileName, string json, string worksheetName, bool reviewStyleCell, bool flattenNestedObjects, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
    {
        var sourceData = JsonHelper.JsonArrayToDataTable(json, flattenNestedObjects);
        await DataTableToFileAsync(fileName, sourceData, worksheetName, reviewStyleCell, columnsNickname, columnsRemove);
    }

    public static async Task JsonFileToFileAsync(string fileName, string jsonFilePath, string worksheetName, bool reviewStyleCell, bool flattenNestedObjects = true)
        => await JsonFileToFileAsync(fileName, jsonFilePath, worksheetName, reviewStyleCell, flattenNestedObjects, null);

    public static async Task JsonFileToFileAsync(string fileName, string jsonFilePath, string worksheetName, bool reviewStyleCell, bool flattenNestedObjects, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
    {
        var json = await File.ReadAllTextAsync(jsonFilePath);
        await JsonToFileAsync(fileName, json, worksheetName, reviewStyleCell, flattenNestedObjects, columnsNickname, columnsRemove);
    }

    public static async Task ExportToExcelAsync<T>(this List<T> sourceData, string fileName, string worksheetName, bool reviewStyleCell)
        => await ListToFileAsync<T>(fileName, sourceData, worksheetName, reviewStyleCell, null);

    public static async Task ExportToExcelAsync<T>(this List<T> sourceData, string fileName, string worksheetName, bool reviewStyleCell, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
        => await ListToFileAsync<T>(fileName, sourceData, worksheetName, reviewStyleCell, columnsNickname, columnsRemove);

    public static async Task ExportToExcelAsync(this DataTable sourceData, string fileName, string worksheetName, bool reviewStyleCell)
        => await DataTableToFileAsync(fileName, sourceData, worksheetName, reviewStyleCell);

    public static async Task ExportToExcelAsync(this DataTable sourceData, string fileName, string worksheetName, bool reviewStyleCell, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
        => await DataTableToFileAsync(fileName, sourceData, worksheetName, reviewStyleCell, columnsNickname, columnsRemove);

    public static async Task ExportJsonToExcelAsync(this string json, string fileName, string worksheetName, bool reviewStyleCell, bool flattenNestedObjects = true)
        => await JsonToFileAsync(fileName, json, worksheetName, reviewStyleCell, flattenNestedObjects);

    public static async Task ExportJsonToExcelAsync(this string json, string fileName, string worksheetName, bool reviewStyleCell, bool flattenNestedObjects, List<ExcelWrapperColumnNickname>? columnsNickname, params int[] columnsRemove)
        => await JsonToFileAsync(fileName, json, worksheetName, reviewStyleCell, flattenNestedObjects, columnsNickname, columnsRemove);
}
