using System.Data;
using MySql.Data.MySqlClient;
using Wanadi.Common.Contracts.PropertyMappers;
using Wanadi.Common.Extensions;
using Wanadi.MySql.Contracts;
using Wanadi.MySql.Contracts.DataWrapper;

namespace Wanadi.MySql.Wrappers;

public static class MySqlWrapper
{
    public static bool ForceMaxConnectionTimeout = false;

    public static string BuildConnectionString(MySqlConnectionSettings settings, string database)
        => BuildConnectionString(settings.Server, database, settings.UserID, settings.Password, settings.Port ?? 3306, settings.CommandTimeout ?? 180, settings.MaximumPoolSize ?? 5000, settings.AllowLoadLocalInfile ?? true);

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
    public static string BuildConnectionString(string server, string database, string userID, string password, uint port = 3306, uint commandTimeout = 180, uint maximumPoolSize = 500, bool allowLoadLocalInfile = true)
           => new MySqlConnectionStringBuilder
           {
               AllowUserVariables = true,
               Server = server,
               Database = database,
               UserID = userID,
               Password = password,
               Port = port,
               SslMode = MySqlSslMode.Disabled,
               PersistSecurityInfo = true,
               ConvertZeroDateTime = true,
               AllowZeroDateTime = true,
               IncludeSecurityAsserts = true,
               MaximumPoolSize = maximumPoolSize,
               DefaultCommandTimeout = commandTimeout,
               ConnectionTimeout = commandTimeout,
               AllowLoadLocalInfile = allowLoadLocalInfile
           }.ConnectionString;

    public static async Task<MySqlConnection> GetConnectionAsync(string connectionString)
    {
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    #region [SelectQuery]

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
        => SelectQueryAsync<TType>(connectionString, commandQuery).GetAwaiter().GetResult();

    public static List<TType> SelectQuery<TType>(MySqlConnection connection, string commandQuery) where TType : class
        => SelectQueryAsync<TType>(connection, commandQuery).GetAwaiter().GetResult();

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
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryAsync<TType>(connection, commandQuery);
        }
    }

    public static async Task<List<TType>> SelectQueryAsync<TType>(MySqlConnection connection, string commandQuery) where TType : class
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                    return new List<TType>();

                var resultFields = GetResultFields<TType>(reader);
                var response = new List<TType>();

                while (await reader.ReadAsync())
                {
                    response.Add(ConvertDataReaderToClass<TType>(reader, resultFields));
                }

                return response;
            }
        }
    }

    #endregion [SelectQuery]

    #region [SelectQueryByEntity]

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
    public static List<TType> SelectQueryByEntity<TType>(string connectionString, int? limit = null) where TType : class
        => SelectQueryByEntityAsync<TType>(connectionString, limit).GetAwaiter().GetResult();

    public static List<TType> SelectQueryByEntity<TType>(MySqlConnection connection, int? limit = null) where TType : class
        => SelectQueryByEntityAsync<TType>(connection, limit).GetAwaiter().GetResult();

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
    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(string connectionString, int? limit = null) where TType : class
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryByEntityAsync<TType>(connection, limit);
        }
    }

    public static async Task<List<TType>> SelectQueryByEntityAsync<TType>(MySqlConnection connection, int? limit = null) where TType : class
    {
        var commandQuery = $"SELECT * FROM `{typeof(TType).GetTableName()}`";

        if (limit.GetValueOrDefault(0) > 0)
            commandQuery += $" LIMIT {limit.GetValueOrDefault(0)}";

        commandQuery += ";";

        return await SelectQueryAsync<TType>(connection, commandQuery);
    }

    #endregion [SelectQueryByEntity]

    #region [SelectQueryFirstOrDefault]

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
        => SelectQueryFirstOrDefaultAsync<TType>(connectionString, commandQuery).GetAwaiter().GetResult();

    public static TType? SelectQueryFirstOrDefault<TType>(MySqlConnection connection, string commandQuery) where TType : class
        => SelectQueryFirstOrDefaultAsync<TType>(connection, commandQuery).GetAwaiter().GetResult();

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
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await SelectQueryFirstOrDefaultAsync<TType>(connection, commandQuery);
        }
    }

    public static async Task<TType?> SelectQueryFirstOrDefaultAsync<TType>(MySqlConnection connection, string commandQuery) where TType : class
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new MySqlCommand(commandQuery, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                    return null;

                var resultFields = GetResultFields<TType>(reader);

                while (await reader.ReadAsync())
                {
                    return ConvertDataReaderToClass<TType>(reader, resultFields);
                }
            }
        }

        return null;
    }

    #endregion [SelectQueryFirstOrDefault]

    #region [ExecuteNonQuery]

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
        => ExecuteNonQueryAsync(connectionString, commandExecute).GetAwaiter().GetResult();

    public static int ExecuteNonQuery(MySqlConnection connection, string commandExecute)
        => ExecuteNonQueryAsync(connection, commandExecute).GetAwaiter().GetResult();

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
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await ExecuteNonQueryAsync(connection, commandExecute);
        }
    }

    public static async Task<int> ExecuteNonQueryAsync(MySqlConnection connection, string commandExecute)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            return await command.ExecuteNonQueryAsync();
        }
    }

    #endregion [ExecuteNonQuery]

    #region [ExecuteScalar]

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
    public static object? ExecuteScalar(string connectionString, string commandExecute)
        => ExecuteScalarAsync(connectionString, commandExecute).GetAwaiter().GetResult();

    public static object? ExecuteScalar(MySqlConnection connection, string commandExecute)
        => ExecuteScalarAsync(connection, commandExecute).GetAwaiter().GetResult();

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
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return ExecuteScalarAsync(connection, commandExecute);
        }
    }

    public static async Task<object?> ExecuteScalarAsync(MySqlConnection connection, string commandExecute)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new MySqlCommand(commandExecute, connection))
        {
            command.CommandType = CommandType.Text;

            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            return await command.ExecuteScalarAsync();
        }
    }

    #endregion [ExecuteScalar]

    #region [Fill]

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
        => FillAsync(connectionString, commandExecute).GetAwaiter().GetResult();

    public static DataTable Fill(MySqlConnection connection, string commandExecute)
        => FillAsync(connection, commandExecute).GetAwaiter().GetResult();

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
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await FillAsync(connection, commandExecute);
        }
    }

    public static async Task<DataTable> FillAsync(MySqlConnection connection, string commandExecute)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using (var command = new MySqlCommand(commandExecute, connection))
        {
            if (ForceMaxConnectionTimeout)
                command.CommandTimeout = 0;

            using (var dataAdapter = new MySqlDataAdapter(command))
            {
                var response = new DataTable();
                await dataAdapter.FillAsync(response);
                return response;
            }
        }
    }

    #endregion [Fill]

    #region [ExecuteBatchesCommandAsync]

    public static async Task<int> BatchInsertAsync(string connectionString, List<BatchCommand> batches)
    {
        using (var connection = await GetConnectionAsync(connectionString))
        {
            return await BatchInsertAsync(connection, batches);
        }
    }

    public static async Task<int> BatchInsertAsync(MySqlConnection connection, List<BatchCommand> batches)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        int response = 0;
        int batchCounter = 0;
        foreach (var batch in batches)
        {
            batchCounter++;
            $"Running batch command {batchCounter:N0} of {batches.Count:N0}".PrintInfo();
            response += await ExecuteNonQueryAsync(connection, batch.MySqlCommand);
        }

        return response;
    }

    #endregion [ExecuteBatchesCommandAsync]

    public static async Task BulkInsertAsync<TType>(string connectionString, List<TType> source, params string[] fieldsIgnore) where TType : class
    {
        if (source == null || source.Count == 0)
            return;

        var tableName = source.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var dataSource = DataWrapper.CastListToDataTableBulkInsert(source, fieldsIgnore);
        if (dataSource == null)
            return;

        //SHOW GLOBAL VARIABLES LIKE 'local_infile';
        //SET GLOBAL local_infile = 1; to enable bulk insert

        using (var connection = new MySqlConnector.MySqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var bulkCopy = new MySqlConnector.MySqlBulkCopy(connection);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.ColumnMappings.AddRange(GetMySqlColumnMapping(dataSource));
            await bulkCopy.WriteToServerAsync(dataSource);
        }
    }

    private static List<MySqlConnector.MySqlBulkCopyColumnMapping> GetMySqlColumnMapping(DataTable dataTable)
    {
        var response = new List<MySqlConnector.MySqlBulkCopyColumnMapping>();

        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            response.Add(new MySqlConnector.MySqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
        }

        return response;
    }

    public static T ConvertDataReaderToClass<T>(MySqlDataReader reader, List<ColumnsResultDataType> resultFields) where T : class
    {
        T response = Activator.CreateInstance<T>();

        foreach (var resultField in resultFields)
        {
            object? value = null;

            try
            {
                if (resultField.DataType == typeof(Int16))
                    value = reader.GetInt16(resultField.ColumnIndex);

                if (resultField.DataType == typeof(Int32))
                    value = reader.GetInt32(resultField.ColumnIndex);

                if (resultField.DataType == typeof(Int64))
                    value = reader.GetInt64(resultField.ColumnIndex);

                if (resultField.DataType == typeof(byte))
                    value = reader.GetByte(resultField.ColumnIndex);

                if (resultField.DataType == typeof(string))
                    value = reader.GetString(resultField.ColumnIndex);

                if (resultField.DataType == typeof(decimal))
                    value = reader.GetDecimal(resultField.ColumnIndex);

                if (resultField.DataType == typeof(double))
                    value = reader.GetDouble(resultField.ColumnIndex);

                if (resultField.DataType == typeof(float))
                    value = reader.GetFloat(resultField.ColumnIndex);

                if (resultField.DataType == typeof(DateTime))
                    value = reader.GetDateTime(resultField.ColumnIndex);

                if (resultField.DataType == typeof(TimeSpan))
                    value = reader.GetTimeSpan(resultField.ColumnIndex);

                if (resultField.DataType == typeof(bool))
                    value = reader.GetBoolean(resultField.ColumnIndex);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Data is Null. This method or property cannot be called on Null values.")
                    continue;

                throw;
            }

            if (value == null || value == DBNull.Value)
                continue;

            var dataType = Nullable.GetUnderlyingType(resultField.Property.PropertyType) ?? resultField.Property.PropertyType;

            if (dataType == typeof(Guid))
            {
                resultField.Property.SetValue(response, Guid.Parse(value.ToString()), null);
                continue;
            }

            if (dataType == typeof(char[]) && resultField.DataType == typeof(string))
            {
                resultField.Property.SetValue(response, value.ToString().ToCharArray(), null);
                continue;
            }

            resultField.Property.SetValue(response, value, null);
        }

        return response;
    }

    private static List<ColumnsResultDataType> GetResultFields<TType>(MySqlDataReader dataReader) where TType : class
    {
        var properties = typeof(TType).GetProperties().Select(t => new PropertyDataType(t)).ToList();

        var response = new List<ColumnsResultDataType>();

        for (var i = 0; i < dataReader.FieldCount; i++)
        {
            var dataReaderField = new ColumnsResultDataType(dataReader, i);

            var property = properties.FirstOrDefault(t => t.ColumnName == dataReaderField.ColumnName);
            if (property != null)
                response.Add(dataReaderField.SetPropertyInfo(property.PropertyInfo));
        }

        return response.Where(t => t.Property != null).ToList();
    }
}