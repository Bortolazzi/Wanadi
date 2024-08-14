namespace Wanadi.Common.Attributes;

public sealed class ExcelHeaderAttribute : Attribute
{
    public List<string> ColumnNames { get; set; } = new List<string>();
    public Type TypeToConvert { get; set; }

    public ExcelHeaderAttribute(Type typeToConvert, params string[] columnNames)
    {
        TypeToConvert = typeToConvert;

        for (int i = 0; i < columnNames.Length; i++)
        {
            var name = columnNames[i].ToLower().RemoveAccents().Replace("\n", " ").Replace(" ", "_").Trim();
            ColumnNames.Add(name);
        }
    }
}