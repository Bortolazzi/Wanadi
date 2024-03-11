using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using MySql.Data.MySqlClient;
using Wanadi.Common.Extensions;
using Wanadi.MySql.Contracts.DataWrapper;

namespace Wanadi.MySql.Wrappers;

public static class MySqlWrapper
{
    public static bool ForceMaxConnectionTimeout = false;

    /// <summary>
    ///     <para>
    ///         pt-BR: Opcional a utilização, mas se utilizado os métodos dessa classe podem ser chamado com a sobrecarga que suprime a ConnectionString.
    ///     </para>
    ///     <para>
    ///         en-US: Optional to use, but if used, the methods of this class can be called with the overload that suppresses the ConnectionString.
    ///     </para>
    /// </summary>
    public static string ConnectionString = string.Empty;

    /// <summary>
    ///     <para>
    ///         pt-BR: Cria uma string de conexão baseado nos parâmetros informados.
    ///     </para>
    ///     <para>
    ///         en-US: Creates a connection string based on the given parameters.
    ///     </para>
    /// </summary>
    /// <param name="server">
    ///     <para>
    ///         pt-BR: IP do servidor MySql ou servername.
    ///     </para>
    ///     <para>
    ///         en-US: MySql server IP or servername.
    ///     </para>
    /// </param>
    /// <param name="database">
    ///     <para>
    ///         pt-BR: Nome do banco de dados para conectar.
    ///     </para>
    ///     <para>
    ///         en-US: Database name to connect.
    ///     </para>
    /// </param>
    /// <param name="user">
    ///     <para>
    ///         pt-BR: Nome do usuário Mysql.
    ///     </para>
    ///     <para>
    ///         en-US: MySql username. 
    ///     </para>
    /// </param>
    /// <param name="password">
    ///     <para>
    ///         pt-BR: Senha do usuário MySql.
    ///     </para>
    ///     <para>
    ///         en-US: MySql user password.
    ///     </para>
    /// </param>
    /// <param name="commandTimeout">
    ///     <para>
    ///         Default 180 ms.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Connection string padronizada para o Mysql.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string standardized for Mysql.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         allowuservariables=True;server=localhost;database=mydatabase;user id=root;password=123456;port=3306;sslmode=None;persistsecurityinfo=True;convertzerodatetime=True;allowzerodatetime=True;includesecurityasserts=True;maxpoolsize=5000;defaultcommandtimeout=180;connectiontimeout=180
    ///     </code>
    /// </returns>
    public static string BuildConnectionString(string server, string database, string user, string password, uint commandTimeout = 180)
           => new MySqlConnectionStringBuilder
           {
               AllowUserVariables = true,
               Server = server,
               Database = database,
               UserID = user,
               Password = password,
               Port = 3306,
               SslMode = MySqlSslMode.Disabled,
               PersistSecurityInfo = true,
               ConvertZeroDateTime = true,
               AllowZeroDateTime = true,
               IncludeSecurityAsserts = true,
               MaximumPoolSize = 5000,
               DefaultCommandTimeout = commandTimeout,
               ConnectionTimeout = commandTimeout
           }.ConnectionString;

    /// <summary>
    ///     <para>
    ///         pt-BR: Método genérico para execução de consultas no MySql. Executa a consulta e converte os campos resultantes em uma lista de registros tipados de acordo com o informado na chamada do método.
    ///     </para>
    ///     <para>
    ///         en-US: Generic method for executing queries in MySql. Executes the query and converts the resulting fields into a list of records typed according to what was informed in the method call.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static List<TType> SelectQuery<TType>(string commandQuery) where TType : class
        => SelectQuery<TType>(ConnectionString, commandQuery);

    /// <summary>
    ///     <para>
    ///         pt-BR: Método genérico para execução de consultas no MySql. Executa a consulta e converte os campos resultantes em uma lista de registros tipados de acordo com o informado na chamada do método.
    ///     </para>
    ///     <para>
    ///         en-US: Generic method for executing queries in MySql. Executes the query and converts the resulting fields into a list of records typed according to what was informed in the method call.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static List<TType> SelectQuery<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            command.Connection.Open();

            using (MySqlDataReader dataReader = command.ExecuteReader())
            {
                if (!dataReader.HasRows)
                    return new List<TType>();

                var resultFields = GetResultFields(dataReader);
                var response = new List<TType>();

                while (dataReader.Read())
                {
                    response.Add(ConvertDataReaderToClass<TType>(dataReader, resultFields));
                }

                return response;
            }
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Executa uma consulta simples sem condições e relacionamento na tabela através do TableAttribute ou object.Name.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a simple query without conditions and relationships on the table using TableAttribute or object.Name.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="limit">
    ///     <para>
    ///         pt-BR: Default -1. Quando maior que zero irá limitar a quantidade de registros de acordo com o valor informado.
    ///     </para
    ///     <para>
    ///         en-US: Default -1. When greater than zero, it will limit the number of records according to the value entered.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static List<TType> SelectQueryByEntity<TType>(int limit = -1) where TType : class
        => SelectQueryByEntity<TType>(ConnectionString, limit);

    /// <summary>
    ///     <para>
    ///         pt-BR: Executa uma consulta simples sem condições e relacionamento na tabela através do TableAttribute ou object.Name.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a simple query without conditions and relationships on the table using TableAttribute or object.Name.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="limit">
    ///     <para>
    ///         pt-BR: Default -1. Quando maior que zero irá limitar a quantidade de registros de acordo com o valor informado.
    ///     </para
    ///     <para>
    ///         en-US: Default -1. When greater than zero, it will limit the number of records according to the value entered.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static List<TType> SelectQueryByEntity<TType>(string connectionString, int limit = -1) where TType : class
    {
        var commandQuery = $"SELECT * FROM `{typeof(TType).GetTableName()}`";

        if (limit > 0)
            commandQuery += $" LIMIT {limit}";

        commandQuery += ";";

        return SelectQuery<TType>(connectionString, commandQuery);
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma consulta e encerra a leitura após carregar e converter o primeiro registro.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a query and ends reading after loading and converting the first record.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Primeiro registro resultante do comando ou nulo caso o comando não gere resultado.
    ///     </para
    ///     <para>
    ///         en-US: First record resulting from the command or null if the command does not generate a result.
    ///     </para>
    /// </returns>
    public static TType? SelectQueryFirstOrDefault<TType>(string commandQuery) where TType : class
        => SelectQueryFirstOrDefault<TType>(ConnectionString, commandQuery);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma consulta e encerra a leitura após carregar e converter o primeiro registro.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a query and ends reading after loading and converting the first record.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Primeiro registro resultante do comando ou nulo caso o comando não gere resultado.
    ///     </para
    ///     <para>
    ///         en-US: First record resulting from the command or null if the command does not generate a result.
    ///     </para>
    /// </returns>
    public static TType? SelectQueryFirstOrDefault<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            command.Connection.Open();

            using (MySqlDataReader dataReader = command.ExecuteReader())
            {
                if (!dataReader.HasRows)
                    return null;

                var resultFields = GetResultFields(dataReader);

                if (dataReader.Read())
                    return ConvertDataReaderToClass<TType>(dataReader, resultFields);
            }
        }

        return null;
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Método genérico para execução de consultas no MySql. Executa a consulta e converte os campos resultantes em uma lista de registros tipados de acordo com o informado na chamada do método.
    ///     </para>
    ///     <para>
    ///         en-US: Generic method for executing queries in MySql. Executes the query and converts the resulting fields into a list of records typed according to what was informed in the method call.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static async Task<List<TType>> SelectQueryAsync<TType>(string commandQuery) where TType : class
        => await SelectQueryAsync<TType>(commandQuery);

    /// <summary>
    ///     <para>
    ///         pt-BR: Método genérico para execução de consultas no MySql. Executa a consulta e converte os campos resultantes em uma lista de registros tipados de acordo com o informado na chamada do método.
    ///     </para>
    ///     <para>
    ///         en-US: Generic method for executing queries in MySql. Executes the query and converts the resulting fields into a list of records typed according to what was informed in the method call.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static async Task<List<TType>> SelectQueryAsync<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            await command.Connection.OpenAsync();

            using (var dataReader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (!dataReader.HasRows)
                    return new List<TType>();

                var resultFields = GetResultFields(dataReader);
                var response = new List<TType>();

                while (await dataReader.ReadAsync())
                {
                    response.Add(MySqlWrapper.ConvertDataReaderToClass<TType>(dataReader, resultFields));
                }

                return response;
            }
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Executa uma consulta simples sem condições e relacionamento na tabela através do TableAttribute ou object.Name.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a simple query without conditions and relationships on the table using TableAttribute or object.Name.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="limit">
    ///     <para>
    ///         pt-BR: Default -1. Quando maior que zero irá limitar a quantidade de registros de acordo com o valor informado.
    ///     </para
    ///     <para>
    ///         en-US: Default -1. When greater than zero, it will limit the number of records according to the value entered.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(int limit = -1) where TType : class
        => await SelectQueryByEntityAsync<TType>(ConnectionString, limit);

    /// <summary>
    ///     <para>
    ///         pt-BR: Executa uma consulta simples sem condições e relacionamento na tabela através do TableAttribute ou object.Name.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a simple query without conditions and relationships on the table using TableAttribute or object.Name.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="limit">
    ///     <para>
    ///         pt-BR: Default -1. Quando maior que zero irá limitar a quantidade de registros de acordo com o valor informado.
    ///     </para
    ///     <para>
    ///         en-US: Default -1. When greater than zero, it will limit the number of records according to the value entered.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Lista de registros recuperados do banco e tipados de acordo com o TType.
    ///     </para>
    ///     <para>
    ///         en-US: List of records retrieved from the database and typed according to TType.
    ///     </para>
    /// </returns>
    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(string connectionString, int limit = -1) where TType : class
    {
        var commandQuery = $"SELECT * FROM `{typeof(TType).GetTableName()}`";

        if (limit > 0)
            commandQuery += $" LIMIT {limit}";

        commandQuery += ";";

        return await SelectQueryAsync<TType>(connectionString, commandQuery);
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma consulta e encerra a leitura após carregar e converter o primeiro registro.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a query and ends reading after loading and converting the first record.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Primeiro registro resultante do comando ou nulo caso o comando não gere resultado.
    ///     </para
    ///     <para>
    ///         en-US: First record resulting from the command or null if the command does not generate a result.
    ///     </para>
    /// </returns>
    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(string commandQuery) where TType : class
        => await SelectQueryFirstOrDefaultAsync<TType>(ConnectionString, commandQuery);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma consulta e encerra a leitura após carregar e converter o primeiro registro.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a query and ends reading after loading and converting the first record.
    ///     </para>
    /// </summary>
    /// <typeparam name="TType">
    ///     <para>
    ///         pt-BR: Tipo da classe a ser carregada com o resultado da consulta.
    ///     </para>
    ///     <para>
    ///         en-US: Type of the class to be loaded with the query result.
    ///     </para>
    /// </typeparam>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandQuery">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Primeiro registro resultante do comando ou nulo caso o comando não gere resultado.
    ///     </para
    ///     <para>
    ///         en-US: First record resulting from the command or null if the command does not generate a result.
    ///     </para>
    /// </returns>
    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(string connectionString, string commandQuery) where TType : class
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            await command.Connection.OpenAsync();

            using (var dataReader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (!dataReader.HasRows)
                    return null;

                var resultFields = GetResultFields(dataReader);
                var response = new List<TType>();

                if (await dataReader.ReadAsync())
                    return MySqlWrapper.ConvertDataReaderToClass<TType>(dataReader, resultFields);
            }
        }

        return null;
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the number of affected records.
    ///     </para>
    /// </summary>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Number of affected records.
    ///     </para>
    /// </returns>
    public static int ExecuteNonQuery(string commandExecute)
        => ExecuteNonQuery(ConnectionString, commandExecute);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the number of affected records.
    ///     </para>
    /// </summary>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Number of affected records.
    ///     </para>
    /// </returns>
    public static int ExecuteNonQuery(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            command.Connection.Open();
            return command.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the number of affected records.
    ///     </para>
    /// </summary>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Number of affected records.
    ///     </para>
    /// </returns>
    public static async Task<int> ExecuteNonQueryAsync(string commandExecute)
        => await ExecuteNonQueryAsync(ConnectionString, commandExecute);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the number of affected records.
    ///     </para>
    /// </summary>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Quantidade de registros afetados.
    ///     </para>
    ///     <para>
    ///         en-US: Number of affected records.
    ///     </para>
    /// </returns>
    public static async Task<int> ExecuteNonQueryAsync(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            await command.Connection.OpenAsync();

            return await command.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the first column of the first row as an object
    ///     </para>
    /// </summary>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Returns the first column of the first row as an object
    ///     </para>
    /// </returns>
    public static object ExecuteScalar(string commandExecute)
        => ExecuteScalar(ConnectionString, commandExecute);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the first column of the first row as an object
    ///     </para>
    /// </summary>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Returns the first column of the first row as an object
    ///     </para>
    /// </returns>
    public static object ExecuteScalar(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            command.Connection.Open();
            return command.ExecuteScalar();
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the first column of the first row as an object
    ///     </para>
    /// </summary>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Returns the first column of the first row as an object
    ///     </para>
    /// </returns>
    public static async Task<object?> ExecuteScalarAsync(string commandExecute)
        => await ExecuteScalarAsync(ConnectionString, commandExecute);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and returns the first column of the first row as an object
    ///     </para>
    /// </summary>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Retorna a primeira coluna da primeira linha como um objeto
    ///     </para>
    ///     <para>
    ///         en-US: Returns the first column of the first row as an object
    ///     </para>
    /// </returns>
    public static async Task<object?> ExecuteScalarAsync(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            await command.Connection.OpenAsync();
            return await command.ExecuteScalarAsync();
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e carrega o resultado para um DataTable.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and loads the result into a DataTable.
    ///     </para>
    /// </summary>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: DataTable carregado com o resultado do comando.
    ///     </para>
    ///     <para>
    ///         en-US: DataTable loaded with the result of the command.
    ///     </para>
    /// </returns>
    public static DataTable Fill(string commandExecute)
        => Fill(ConnectionString, commandExecute);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e carrega o resultado para um DataTable.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and loads the result into a DataTable.
    ///     </para>
    /// </summary>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: DataTable carregado com o resultado do comando.
    ///     </para>
    ///     <para>
    ///         en-US: DataTable loaded with the result of the command.
    ///     </para>
    /// </returns>
    public static DataTable Fill(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            connection.Open();

            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command))
            {
                var response = new DataTable();
                dataAdapter.Fill(response);
                return response;
            }
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e carrega o resultado para um DataTable.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and loads the result into a DataTable.
    ///     </para>
    /// </summary>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: DataTable carregado com o resultado do comando.
    ///     </para>
    ///     <para>
    ///         en-US: DataTable loaded with the result of the command.
    ///     </para>
    /// </returns>
    public static async Task<DataTable> FillAsync(string commandExecute)
        => await FillAsync(ConnectionString, commandExecute);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a execução de uma query e carrega o resultado para um DataTable.
    ///     </para>
    ///     <para>
    ///         en-US: Executes a query and loads the result into a DataTable.
    ///     </para>
    /// </summary>
    /// <param name="connectionString">
    ///     <para>
    ///         pt-BR: String de conexão a ser utilizada na execução do comando.
    ///     </para>
    ///     <para>
    ///         en-US: Connection string to be used when executing the command.
    ///     </para>
    /// </param>
    /// <param name="commandExecute">
    ///     <para>
    ///         pt-BR: Comando a ser executado.
    ///     </para>
    ///     <para>
    ///         en-US: Command to be executed.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: DataTable carregado com o resultado do comando.
    ///     </para>
    ///     <para>
    ///         en-US: DataTable loaded with the result of the command.
    ///     </para>
    /// </returns>
    public static async Task<DataTable> FillAsync(string connectionString, string commandExecute)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        using (MySqlCommand command = new MySqlCommand(commandExecute, connection))
        {
            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            await connection.OpenAsync();

            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command))
            {
                var response = new DataTable();
                await dataAdapter.FillAsync(response);
                return response;
            }
        }
    }

    public static async Task<int> ExecuteBatchesCommandAsync(string connectionString, List<BatchCommand> batches)
    {
        int response = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            int batchCounter = 0;
            foreach (var batch in batches)
            {
                batchCounter++;
                $"Running batch command {batchCounter:N0} of {batches.Count:N0}".PrintInfo();
                using (MySqlCommand command = new MySqlCommand(batch.MySqlCommand, connection))
                {
                    command.CommandType = CommandType.Text;

                    if (ForceMaxConnectionTimeout)
                        command.CommandTimeout = 0;

                    response += await command.ExecuteNonQueryAsync();
                }
            }
        }

        return response;
    }

    public static T ConvertDataReaderToClass<T>(MySqlDataReader reader, List<string> resultFields) where T : class
    {
        T response = Activator.CreateInstance<T>();
        var properties = response.GetType().GetProperties();

        foreach (var propertyInfo in properties)
        {
            var columnAttribute = propertyInfo.GetAttribute<ColumnAttribute>();
            var columnName = columnAttribute?.Name ?? propertyInfo.Name;

            if (!resultFields.Contains(columnName))
                continue;

            object value = reader[columnName];

            if (value == DBNull.Value || value == null)
                continue;

            var dataType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            if (dataType == typeof(Guid) && value.GetType() == typeof(string))
            {
                propertyInfo.SetValue(response, Guid.Parse(value.ToString()), null);
                continue;
            }

            if (dataType == typeof(DateTime) || dataType == typeof(Boolean))
            {
                propertyInfo.SetValue(response, Convert.ChangeType(value, dataType), null);
                continue;
            }

            propertyInfo.SetValue(response, value, null);
        }

        return response;
    }

    private static List<string> GetResultFields(MySqlDataReader dataReader)
        => Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName).ToList();
}