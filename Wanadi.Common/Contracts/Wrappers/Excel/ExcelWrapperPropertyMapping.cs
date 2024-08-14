using System.Reflection;

namespace Wanadi.Common.Contracts.Wrappers.Excel;

public sealed record ExcelWrapperPropertyMapping
{
    public ExcelWrapperPropertyMapping(
        PropertyInfo property,
        string propertyName,
        int columnIndex,
        Type typeToConvert)
    {
        Property = property;
        PropertyName = propertyName;
        ColumnIndex = columnIndex;
        TypeToConvert = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
    }

    public PropertyInfo Property { get; set; }
    public string PropertyName { get; set; }
    public int ColumnIndex { get; set; }
    public Type TypeToConvert { get; set; }
}