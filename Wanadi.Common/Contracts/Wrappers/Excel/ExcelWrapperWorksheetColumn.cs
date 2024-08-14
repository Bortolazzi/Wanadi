namespace Wanadi.Common.Contracts.Wrappers.Excel;

public sealed record ExcelWrapperWorksheetColumn
{
    public ExcelWrapperWorksheetColumn() { }
    public ExcelWrapperWorksheetColumn(int columnIndex, object columnName)
    {
        ColumnIndex = columnIndex;
        ColumnName = columnName.ToString().ToLower().Trim().RemoveAccents().Replace("\n", " ").Replace(" ", "_").Trim();
    }

    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; }
}