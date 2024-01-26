namespace Wanadi.MySql.Contracts.DataWrapper;

public class BatchCommand
{
	public BatchCommand(int count, string mySqlCommand)
	{
		Count = count;
		MySqlCommand = mySqlCommand;
	}

    public BatchCommand(List<InsertCommand> insertCommands, bool disableKeyCheks)
    {
		Count = insertCommands.Count;
		MySqlCommand = $"{insertCommands.FirstOrDefault().PrefixCommand}{string.Join(",", insertCommands.Select(t => t.SuffixCommand))};";

		if(disableKeyCheks)
            MySqlCommand = $"SET foreign_key_checks = 0; {MySqlCommand} SET foreign_key_checks = 1;";
    }

    public int Count { get; set; }
	public string MySqlCommand { get; set; }
}