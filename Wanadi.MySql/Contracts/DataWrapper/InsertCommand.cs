namespace Wanadi.MySql.Contracts.DataWrapper;

public class InsertCommand
{
    public InsertCommand(string tableName, string prefixCommand, List<ValueProperty> values)
    {
        TableName = tableName;
        PrefixCommand = prefixCommand;
        SuffixCommand = $"({string.Join(",", values.OrderBy(t => t.ColumnName).Select(t => t.Value))})";
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Nome da tabela para qual o comando foi criado.
    ///     </para>
    ///     <para>
    ///         en-US: Name of the table for which the command was created.
    ///     </para>
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Prefixo do comando de inseção.
    ///     </para>
    ///     <para>
    ///         en-US: Insert command prefix.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         INSERT INTO `table` (`field1`, `field2`) VALUES
    ///     </code>
    /// </summary>
    public string PrefixCommand { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Sufixo do comando de inseção contendo os valores a serem inseridos.
    ///     </para>
    ///     <para>
    ///         en-US: Suffix of the insert command containing the values to be inserted.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         ('value1', 'value2')
    ///     </code>
    /// </summary>
    public string SuffixCommand { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Comando de inserção completo.
    ///     </para>
    ///     <para>
    ///         en-US: Full insert command.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///        INSERT INTO `table` (`field1`, `field2`) VALUES ('value1', 'value2');
    ///     </code>
    /// </summary>
    public string MySqlCommand => $"{PrefixCommand}{SuffixCommand};";

    /// <summary>
    ///     <para>
    ///         pt-BR: Comando de inserção completo seguido do comando de recuperar o último id inserido na tabela.
    ///     </para>
    ///     <para>
    ///         en-US: Full insertion command followed by the command to retrieve the last id inserted in the table.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///        INSERT INTO `table` (`field1`, `field2`) VALUES ('value1', 'value2'); SELECT LAST_INSERT_ID();
    ///     </code>
    /// </summary>
    public string MySqlCommandGetId => $"{MySqlCommand} SELECT LAST_INSERT_ID();";
}