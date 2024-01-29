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
        MySqlCommand = $"{insertCommands.FirstOrDefault()?.PrefixCommand}{string.Join(",", insertCommands.Select(t => t.SuffixCommand))};";

        if (disableKeyCheks)
            MySqlCommand = $"SET foreign_key_checks = 0; {MySqlCommand} SET foreign_key_checks = 1;";
    }

    /// <summary>
    ///		<para>
    ///			pt-BR: Quantidade de registros contidos no comando em lote.
    ///		</para>
    ///		<para>
    ///			en-US: Number of records contained in the batch command.
    ///		</para>
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Lote de comandos de inserção.
    ///     </para>
    ///     <para>
    ///         en-US: Batch of insert commands.
    ///     </para>
    /// </summary>
	public string MySqlCommand { get; set; }
}