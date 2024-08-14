namespace Wanadi.Common.Contracts.Wrappers.Excel;

public sealed record ExcelWrapperColumnNickname(string columnName, string columnNickname)
{
    public string ColumnName { get; set; } = columnName;
    public string ColumnNickname { get; set; } = columnNickname;
}