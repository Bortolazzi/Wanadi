namespace Wanadi.MySql.Contracts.DataWrapper;

public class InsertCommand
{
    public InsertCommand(string tableName, string prefixCommand, List<ValueProperty> values)
    {
        TableName = tableName;
        PrefixCommand = prefixCommand;
        SuffixCommand = $"({string.Join(",", values.OrderBy(t => t.ColumnName).Select(t => t.Value))})";
    }

    public string TableName { get; set; }
    public string PrefixCommand { get; set; }
    public string SuffixCommand { get; set; }
    public string MySqlCommand => $"{PrefixCommand}{SuffixCommand};";
    public string MySqlCommandGetId => $"{MySqlCommand} SELECT LAST_INSERT_ID();";
}