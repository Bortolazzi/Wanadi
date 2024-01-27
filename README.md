## Sobre:  

Projeto desenvolvido para encapsular algumas funções que facilitam o dia a dia do desenvolvedor. Estão inclusas algumas funcionalidades básicas como, recuperar atributos de propriedades e objetos, operações em banco de dados MySql, gerar scripts de inserção dinamicamente, independente de ser um objeto fortemente tipado ou um objeto anônimo. 

A ideia é manter o projeto atualizado e crescendo com algumas funcionalidades que irão ajudar no cotidiano.

* **NuGet Wanadi.Common** https://www.nuget.org/packages/Wanadi.Common
    - Disponível para .net6 e .net8
* **NuGet Wanadi.MySql** https://www.nuget.org/packages/Wanadi.MySql
    - Disponível para .net6 e .net8

--------------------------------------------------------------------------------------------------
## Resultado de teste de inserção.
### Comparação entre a utilização do DataWrapper e Entity Framework Core para Mysql.

> Usando conexão com bando de dados em VPS na Alemanha. Localmente ambos são mais rápidos.

| ORM           | Quantidade de Registros   | Tempo            | 
| :---          | :---                      | :---             |
| DataWrapper   | 1.000                     | 00:00:01.0554050 |
| EF Core       | 1.000                     | 00:00:09.4387130 |
| DataWrapper   | 10.000                    | 00:00:04.3088360 |
| EF Core       | 10.000                    | 00:00:56.7174500 |
| DataWrapper   | 100.000                   | 00:00:52.3876710 |
| EF Core       | 100.000                   | 00:09:36.0190860 |

--------------------------------------------------------------------------------------------------  
## ToDo List:
- [x] Publicar primeira versão dos pacotes
- [X] Criar summaries no código para facilitar a utilização
- [X] Adicionar FileHelper (Para leitura e escrita de arquivos)
- [ ] Adicionar CsvHelper (Para ajudar na leitura de arquivos CSV e conversão para objetos tipados)

--------------------------------------------------------------------------------------------------  
> [!WARNING]
> Código está livre para leitura e críticas, atenção ao usá-lo. Fique à vontade para sugerir mudanças, abrir uma branch para ajudar a melhorá-lo.

--------------------------------------------------------------------------------------------------  
# Wanadi.MySql - Como usar:

# [DataWrapper.cs](./Wanadi.MySql/Wrappers/DataWrapper.cs)

Classe estática que tem foco em criação de scripts de inserção em lote. Com ela é possível transformar uma coleção de instâncias de classes ou até objetos anônimos em comandos de inserção.

--------------------------------------------------------------------------------------------------  

<details>
    <summary>Configurando inserção de propriedades do tipo Guid ou Enum</summary>

## DataWrapper.GuidOption

Enumerador que é utilizado no momento da criação dos comandos de inserção para definir se uma propriedade do tipo Guid será ignorada na inserção ou convertida para uma string/varchar.

| GuidOption       | Ação                                  | 
| :---             |     :---                              |
| IgnoreOnInsert   | Ignora ao gerar o comando de inserção | 
| CastToString     | Converte para string                  | 

> Caso seja selecionado IgnoreOnInsert as propriedades do tipo Guid serão ignoradas ao montar o comando de inserção.

--------------------------------------------------------------------------------------------------  

## DataWrapper.EnumOption

Enumerador que é utilizado no momento da criação dos comandos de inserção para definir se uma propriedade do tipo Enum será ignorada, convertida em int ou string (Usando o Atributo Description, caso não tenha será o texto que representa o enumerador)

| EnumOption       | Ação                                  |
| :---             |     :---                              |
| IgnoreOnInsert   | Ignora ao gerar o comando de inserção |
| CastToString     | Description ou nameof do enum         |
| CastToInt        | Valor inteiro que representa o enum   |



> Caso seja selecionado IgnoreOnInsert as propriedades do tipo Enum serão ignoradas ao montar o comando de inserção.

> Caso seja selecionado CastToString o valor representado no comando de inserção será o contido no DescriptionAttribute, caso esse não esteja presente será utilizado o Name.

</details>

--------------------------------------------------------------------------------------------------  

## DataWrapper.GenerateInsertCommand (Único objeto)
Gera o script de inserção baseado em um objeto, seja ele tipado ou anônimo.

<details>
<summary>Exemplo de código</summary>

```csharp

public enum Status
{
    [Description("Usuário Novo")]
    NewUser = 1,
    OldUser = 2
}

[Table("myTable")]
public class MyClass
{
    ///Será ignorado por ter o Atributo que informa que ele é gerado pelo banco de dados
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("uuid")]
    public Guid Uuid { get; set; }

    [Column("status")]
    public Status Status { get; set; }
}

var obj = new MyClass()
{
    Name = "Renato",
    Uuid = Guid.NewGuid(),
    Status = Status.NewUser
};

DataWrapper.GuidOption = GuidConditions.CastToString;
DataWrapper.EnumOption = EnumConditions.CastToString;

var insertCommand = DataWrapper.GenerateInsertCommand(obj);

```
### Exemplo dos dados retornados no insertCommand baseado no código acima

> Foi informado que era pra converter uma propriedade Guid em varchar.

> Foi informado que era pra recuperar a descrição do enumerador, como o enumerador Status estava como NewUser a descrição dele é `Usuário Novo`.

| Propriedade | Valor |
| :---                             | :---  |
| insertCommand.TableName          | "myTable" |
| insertCommand.PrefixCommand      | "INSERT INTO \`myTable\` (\`name\`, \`uuid\`, \`status\`) VALUES " |
| insertCommand.SuffixCommand      | "('Renato', 'e406f261-75b1-441a-a0f2-848a48f93778', 'Usuário Novo')" |
| insertCommand.MySqlCommand       | "INSERT INTO \`myTable\` (\`name\`, \`uuid\`, \`status\`) VALUES ('Renato', 'e406f261-75b1-441a-a0f2-848a48f93778', 'Usuário Novo');" |
| insertCommand.MySqlCommandGetId  | "INSERT INTO \`myTable\` (\`name\`, \`uuid\`, \`status\`) VALUES ('Renato', 'e406f261-75b1-441a-a0f2-848a48f93778', 'Usuário Novo'); SELECT LAST_INSERT_ID();" |

### Para executar o comando:

```csharp

var obj = new MyClass()
{
    Name = "Renato",
    Uuid = Guid.NewGuid(),
    Status = Status.NewUser
};

DataWrapper.GuidOption = GuidConditions.CastToString;
DataWrapper.EnumOption = EnumConditions.CastToString;

var insertCommand = DataWrapper.GenerateInsertCommand(obj);
var rowsAffected = await MySqlWrapper.ExecuteNonQueryAsync("CONNECTION_STRING", insertCommand.MySqlCommand);
var id = await MySqlWrapper.ExecuteScalarAsync("CONNECTION_STRING", insertCommand.MySqlCommandGetId);

```

### Variações no resultado do script - IgnoreOnInsert:

```csharp

var obj = new MyClass()
{
    Name = "Renato",
    Uuid = Guid.NewGuid(),
    Status = Status.NewUser
};

DataWrapper.GuidOption = GuidConditions.IgnoreOnInsert;
DataWrapper.EnumOption = EnumConditions.IgnoreOnInsert;

var insertCommand = DataWrapper.GenerateInsertCommand(obj);

```

| Propriedade | Valor |
| :---                             | :---  |
| insertCommand.TableName          | "myTable" |
| insertCommand.PrefixCommand      | "INSERT INTO \`myTable\` (\`name\`) VALUES " |
| insertCommand.SuffixCommand      | "('Renato')" |
| insertCommand.MySqlCommand       | "INSERT INTO \`myTable\` (\`name\`) VALUES ('Renato');" |
| insertCommand.MySqlCommandGetId  | "INSERT INTO \`myTable\` (\`name\`) VALUES ('Renato'); SELECT LAST_INSERT_ID();" |

### Variações no resultado do script - EnumOption = CastToInt:

```csharp

var obj = new MyClass()
{
    Name = "Renato",
    Uuid = Guid.NewGuid(),
    Status = Status.NewUser
};

DataWrapper.GuidOption = GuidConditions.IgnoreOnInsert;
DataWrapper.EnumOption = EnumConditions.CastToInt;

var insertCommand = DataWrapper.GenerateInsertCommand(obj);

```

| Propriedade | Valor |
| :---                             | :---  |
| insertCommand.TableName          | "myTable" |
| insertCommand.PrefixCommand      | "INSERT INTO \`myTable\` (\`name\`, \`status\`) VALUES " |
| insertCommand.SuffixCommand      | "('Renato')" |
| insertCommand.MySqlCommand       | "INSERT INTO \`myTable\` (\`name\`, \`status\`) VALUES ('Renato', 1);" |
| insertCommand.MySqlCommandGetId  | "INSERT INTO \`myTable\` (\`name\`, \`status\`) VALUES ('Renato', 1); SELECT LAST_INSERT_ID();" |

### Informando explicitamente o nome da tabela:

```csharp

var obj = new MyClass()
{
    Name = "Renato",
    Uuid = Guid.NewGuid(),
    Status = Status.NewUser
};

DataWrapper.GuidOption = GuidConditions.IgnoreOnInsert;
DataWrapper.EnumOption = EnumConditions.CastToInt;

var insertCommand = DataWrapper.GenerateInsertCommand("myAnotherTable", obj);

```
> Note que o nome recuperado através do TableAttribute não está mais sendo utilizado

| Propriedade | Valor |
| :---                             | :---  |
| insertCommand.TableName          | "myAnotherTable" |
| insertCommand.PrefixCommand      | "INSERT INTO \`myAnotherTable\` (\`name\`, \`status\`) VALUES " |
| insertCommand.SuffixCommand      | "('Renato')" |
| insertCommand.MySqlCommand       | "INSERT INTO \`myAnotherTable\` (\`name\`, \`status\`) VALUES ('Renato', 1);" |
| insertCommand.MySqlCommandGetId  | "INSERT INTO \`myAnotherTable\` (\`name\`, \`status\`) VALUES ('Renato', 1); SELECT LAST_INSERT_ID();" |

### Gerando script de objeto anônimo:

```csharp

var obj = new
{
    Age = 31,
    Name = "Renato",
    LastName = "Bortolazzi Junior",
    Active = true,
    CreatedAt = DateTime.Now
};

var insertCommand = DataWrapper.GenerateInsertCommand("myAnonymousTable", obj);

```

| Propriedade | Valor |
| :---                             | :---  |
| insertCommand.TableName          | "myAnonymousTable" |
| insertCommand.PrefixCommand      | "INSERT INTO \`myAnonymousTable\` (\`Age\`, \`Name\`, \`LastName\`, \`Active\`, \`CreatedAt\`) VALUES " |
| insertCommand.SuffixCommand      | "(31, 'Renato', 'Bortolazzi Junior', 1, '2024-01-26 19\:19\:42')" |
| insertCommand.MySqlCommand       | "INSERT INTO \`myAnonymousTable\` (\`Age\`, \`Name\`, \`LastName\`, \`Active\`, \`CreatedAt\`) VALUES (31, 'Renato', 'Bortolazzi Junior', 1, '2024-01-26 19\:19\:42');" |
| insertCommand.MySqlCommandGetId  | "INSERT INTO \`myAnonymousTable\` (\`Age\`, \`Name\`, \`LastName\`, \`Active\`, \`CreatedAt\`) VALUES (31, 'Renato', 'Bortolazzi Junior', 1, '2024-01-26 19\:19\:42'); SELECT LAST_INSERT_ID();" |

> Atenção, o DataWrapper não cria a tabela, apenas gera um script.

> Recomendado para algum tipo de teste e/ou incrementado por uma geração da tabela dinamicamente também.

</details>

--------------------------------------------------------------------------------------------------  

## DataWrapper.GenerateInsertCommands (Coleção de dados)

Desenvolvido pela necessidade de gerar scripts de inserção em lote, onde o EF e Dapper realizam algumas verificações a mais o que leva no incremento considerável de tempo para a execução da tarefa.
Método recebe uma coleção de dados, tipados ou anônimos, e gera uma coleção de registros (Iguais o recuperado no DataWrapper.GenerateInsertCommand).

<details>
<summary>Exemplo de código</summary>

```csharp

[Table("myTable")]
public class MyClass
{
    public MyClass(string name, bool active)
    {
        Name = name;
        Active = active;
    }

    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("active")]
    public bool Active { get; set; }
}

var listToInsert = new List<MyClass>()
{
    new MyClass("Renato", true),
    new MyClass("Bortolazzi", true),
    new MyClass("Junior", true),
    new MyClass("Juninho", false),
};

var sourceInserts1 = DataWrapper.GenerateInsertCommands(listToInsert);
//MySqlCommands:
//#1 - INSERT INTO `myTable` (`id`, `name`, `active`) VALUES (0, 'Renato', 1);
//#2 - INSERT INTO `myTable` (`id`, `name`, `active`) VALUES (0, 'Bortolazzi', 1);
//#3 - INSERT INTO `myTable` (`id`, `name`, `active`) VALUES (0, 'Junior', 1);
//#4 - INSERT INTO `myTable` (`id`, `name`, `active`) VALUES (0, 'Juninho', 0); 

//Caso necessário, também pode ser alterado o nome da tabela explicitamente
var sourceInserts2 = DataWrapper.GenerateInsertCommands("tableNames", listToInsert);
//MySqlCommands:
//#1 - INSERT INTO `tableNames` (`id`, `name`, `active`) VALUES (0, 'Renato', 1);
//#2 - INSERT INTO `tableNames` (`id`, `name`, `active`) VALUES (0, 'Bortolazzi', 1);
//#3 - INSERT INTO `tableNames` (`id`, `name`, `active`) VALUES (0, 'Junior', 1);
//#4 - INSERT INTO `tableNames` (`id`, `name`, `active`) VALUES (0, 'Juninho', 0); 

//Caso necessário, também pode ser informado o nome das propriedades (Não das colunas) a serem excluídas da instrução
var sourceInserts3 = DataWrapper.GenerateInsertCommands("tableNames", listToInsert, "Id");
//MySqlCommands:
//#1 - INSERT INTO `tableNames` (`name`, `active`) VALUES ('Renato', 1);
//#2 - INSERT INTO `tableNames` (`name`, `active`) VALUES ('Bortolazzi', 1);
//#3 - INSERT INTO `tableNames` (`name`, `active`) VALUES ('Junior', 1);
//#4 - INSERT INTO `tableNames` (`name`, `active`) VALUES ('Juninho', 0); 

```

</details>

--------------------------------------------------------------------------------------------------  

## DataWrapper.GenerateInsertCommandsParallel (Coleção de dados utilizando Parallel)

Única diferença para o método acima é que ao percorrer a coleção de dados é utilizado Parallel.ForEach.

Desenvolvido pela necessidade de gerar scripts de inserção em lote, onde o EF e Dapper realizam algumas verificações a mais o que leva no incremento considerável de tempo para a execução da tarefa.
Método recebe uma coleção de dados, tipados ou anônimos, e gera uma coleção de registros (Iguais o recuperado no DataWrapper.GenerateInsertCommand).

--------------------------------------------------------------------------------------------------  

## DataWrapper.GenerateBatchCommands (Agrupa em lotes os comandos para inserção)

Os métodos acima descritos são para geração de comandos individuais de inserção, já esse realiza a criação dos inserts mas agrupa.

Existem várias sobrecargas desse método, para não precisar passar manualmente pelas etapas anteriores, irei descrever a maneira mais fácil e então coloco abaixo os demais exemplos de chamada.

<details>
    <summary>Exemplo de código</summary>

```csharp

[Table("myTable")]
public class MyClass
{
    public MyClass(string name, bool active)
    {
        Name = name;
        Active = active;
    }

    [Column("name")]
    public string Name { get; set; }

    [Column("active")]
    public bool Active { get; set; }
}

var listToInsert = new List<MyClass>()
{
    new MyClass("Renato", true),
    new MyClass("Bortolazzi", true),
    new MyClass("Junior", true),
    new MyClass("Juninho", false),
};

List<BatchCommand> GenerateBatchCommands(IList sourceItems, int quantityPerBatch, bool disableKeyCheks = false, params string[] fieldsIgnore)
//Ao chamar o método temos que passar:
//IList sourceItems (Obrigatório) - Coleção de dados 
//int quantityPerBatch (Obrigatório) - Número de registros por lote (No exemplo estou usando 2 para mostrar a saída, mas vai de acordo com o que seu MySql suporta) 
//bool disableKeyCheks (Obrigatório - Default false) - Indicador de desabilitação da checagem de chaves de restrição no mysql
//params string[] fieldsIgnore (Opcional) - Caso necessário excluir alguma propriedade da instrução de insert

var sourceInserts1 = DataWrapper.GenerateBatchCommands(listToInsert, 2, false);
//sourceInserts1 receberá 2 registros do tipo BatchCommand
//#1 - BatchCommand - Count = 2, MySqlCommand = "INSERT INTO `myTable` (`name`, `active`) VALUES ('Renato', 1), ('Bortolazzi', 1);
//#2 - BatchCommand - Count = 2, MySqlCommand = "INSERT INTO `myTable` (`name`, `active`) VALUES ('Junior', 1), ('Juninho', 0);

//Caso necessário, também pode ser alterado o nome da tabela explicitamente
var sourceInserts2 = DataWrapper.GenerateBatchCommands("tableName", listToInsert, 2, false);
//sourceInserts2 receberá 2 registros do tipo BatchCommand
//#1 - BatchCommand - Count = 2, MySqlCommand = "INSERT INTO `tableName` (`name`, `active`) VALUES ('Renato', 1), ('Bortolazzi', 1);
//#2 - BatchCommand - Count = 2, MySqlCommand = "INSERT INTO `tableName` (`name`, `active`) VALUES ('Junior', 1), ('Juninho', 0);

//Executará a mesma coisa dos demais mas utilizando Parallel.ForEach
var sourceInserts3 = DataWrapper.GenerateInsertCommandsParallel(listToInsert, 1000, false);
//sourceInserts3 receberá 1 registro do tipo BatchCommand. (Como a coleção não possuí mais que 1000 registros será retornado apenas um)
//#1 - BatchCommand - Count = 4, MySqlCommand = "INSERT INTO `tableName` (`name`, `active`) VALUES ('Renato', 1), ('Bortolazzi', 1), ('Junior', 1), ('Juninho', 0);

```
    
</details>


--------------------------------------------------------------------------------------------------  

# [MySqlWrapper.cs](./Wanadi.MySql/Wrappers/MySqlWrapper.cs)

Classe estática que contém a implementação de alguns métodos que realizam transações no banco de dados de acordo com o script informado na chamada de cada método. Criada para facilitar a leitura de entidades que são abstrações de tabelas de banco de dados ou até mesmo classes criadas para recuperar um select composto.  

O básico para as operações em banco de dados são as connectionString, caso necessário é possível utilizar uma connectionString setada diretamente na classe MySqlWrapper.cs.
> Nos métodos de execução de script em banco há uma sobrecarga que utiliza a MySqlWrapper.ConnectionString como padrão, caso não seja informada na chamada uma outra específica ao chamar os demais métodos.

<details>
<summary>Exemplo de código</summary>

```csharp
MySqlWrapper.ConnectionString = "CONNECTION_STRING";

//Ou pode gerar através do método BuildConnectionString:

MySqlWrapper.ConnectionString = MySqlWrapper.ConnectionString("MYSQL_SERVER", "DATABASE_NAME", "USER", "PASSWORD");
```

Após star a ConnectionString os métodos de execução de script podem ser utilizados suprimindo a connectionString da chamada.

</details>

--------------------------------------------------------------------------------------------------  

### MySqlWrapper.SelectQuery\<TType\>
Método genérico para execução de consultas no MySql. Executa a consulta e converte os campos resultantes em uma lista de registros tipados de acordo com o informado na chamada do método:

<details>
<summary>Exemplo de código</summary>
  
```csharp
var sourceList = MySqlWrapper.SelectQuery<MyClass>("CONNECTION_STRING", "SELECT * FROM myTable");

//Execução informando suprimindo connectionString:
MySqlWrapper.ConnectionString = "CONNECTION_STRING";
var sourceList = MySqlWrapper.SelectQuery<MyClass>("SELECT * FROM myTable");

//Variação assíncrona:
var sourceList = await MySqlWrapper.SelectQueryAsync<MyClass>("CONNECTION_STRING", "SELECT * FROM myTable");

///Suprimindo connectionString
var sourceList = await MySqlWrapper.SelectQueryAsync<MyClass>("SELECT * FROM myTable");
```

</details>

--------------------------------------------------------------------------------------------------  

### MySqlWrapper.SelectQueryByEntity\<TType\>
Realiza uma consulta simples baseado em uma classe. (TableAttribute ou nome da classe)

<details>
<summary>Exemplo de código</summary>
  
```csharp

namespace YourNameSpace;

[Table("myTable")]
public class MyClass
{
  ....
}

//Para recuperar 1.000 registros da tabela "myTable" convertendo o resultado em um objeto do tipo MyClass.
//Caso a classe possua o TableAttribute: será utilizado o valor que foi definido - No caso "myTable"
//Caso a classe não possua o TableAttribute: será utilizado o valor "MyClass"
//Parâmetro limit é opcional, informando -1 irá trazer todos os registros da tabela
var sourceList = MySqlWrapper.SelectQueryByEntity<MyClass>("CONNECTION_STRING", 1000);

///Suprimindo connectionString
var sourceList = MySqlWrapper.SelectQueryByEntity<MyClass>(1000);

//Variação assíncrona:
var sourceList = await MySqlWrapper.SelectQueryByEntityAsync<MyClass>("CONNECTION_STRING", 1000);
var sourceList = await MySqlWrapper.SelectQueryByEntityAsync<MyClass>(1000);
```
</details>

--------------------------------------------------------------------------------------------------  

### MySqlWrapper.SelectQueryFirstOrDefault\<TType\>
Realiza uma consulta e encerra a leitura após carregar e converter o primeiro registro

<details>
<summary>Exemplo de código</summary>
  
```csharp

namespace YourNameSpace;

[Table("myTable")]
public class MyClass
{
    [Column("name")]
    public string Name { get; set; }

    [Column("active")]
    public bool Active { get; set; }    
}

var sourceList = MySqlWrapper.SelectQueryFirstOrDefault<MyClass>("CONNECTION_STRING", "SELECT * FROM myTable WHERE name LIKE '%renato%' AND active = 1;");

///Suprimindo connectionString
var sourceList = MySqlWrapper.SelectQueryFirstOrDefault<MyClass>("SELECT * FROM myTable WHERE name LIKE '%renato%' AND active = 1;");

//Variação assíncrona:
var sourceList = await MySqlWrapper.SelectQueryFirstOrDefaultAsync<MyClass>("CONNECTION_STRING", "SELECT * FROM myTable WHERE name LIKE '%renato%' AND active = 1;");
var sourceList = await MySqlWrapper.SelectQueryFirstOrDefaultAsync<MyClass>("SELECT * FROM myTable WHERE name LIKE '%renato%' AND active = 1;");
```

</details>

--------------------------------------------------------------------------------------------------  

### MySqlWrapper.ExecuteNonQuery
Realiza a execução de uma query e retorna a quantidade de registros afetados

<details>
<summary>Exemplo de código</summary>

```csharp

var rowsAffected = MySqlWrapper.ExecuteNonQuery("CONNECTION_STRING", "UPDATE myTable SET active = 0 WHERE name LIKE '%renato%' AND active = 1;");

///Suprimindo connectionString
var rowsAffected = MySqlWrapper.ExecuteNonQuery("UPDATE myTable SET active = 0 WHERE name LIKE '%renato%' AND active = 1;");

//Variação assíncrona:
var rowsAffected = await MySqlWrapper.ExecuteNonQueryAsync("CONNECTION_STRING", "UPDATE myTable SET active = 0 WHERE name LIKE '%renato%' AND active = 1;");
var rowsAffected = await MySqlWrapper.ExecuteNonQueryAsync("UPDATE myTable SET active = 0 WHERE name LIKE '%renato%' AND active = 1;");
```

</details>

--------------------------------------------------------------------------------------------------  

### MySqlWrapper.ExecuteScalar
Realiza a execução de uma query e retorna a primeira coluna da primeira linha como um objeto

<details>
<summary>Exemplo de código</summary>

```csharp
//Exemplo que insere um registro e retorna o id do registro inserido
var id = MySqlWrapper.ExecuteScalar("CONNECTION_STRING", "INSERT INTO myTable (`name`, `active`) VALUES ('RENATO', 1); SELECT LAST_INSERT_ID();");

///Suprimindo connectionString
var id = MySqlWrapper.ExecuteScalar("INSERT INTO myTable (`name`, `active`) VALUES ('RENATO', 1); SELECT LAST_INSERT_ID();");

//Variação assíncrona:
var id = await MySqlWrapper.ExecuteScalarAsync("CONNECTION_STRING", "INSERT INTO myTable (`name`, `active`) VALUES ('RENATO', 1); SELECT LAST_INSERT_ID();");
var id = await MySqlWrapper.ExecuteScalarAsync("INSERT INTO myTable (`name`, `active`) VALUES ('RENATO', 1); SELECT LAST_INSERT_ID();");
```

</details>

--------------------------------------------------------------------------------------------------  

### MySqlWrapper.Fill
Realiza a execução de uma query e carrega o resultado para um DataTable

<details>
<summary>Exemplo de código</summary>

```csharp
var dataTable = MySqlWrapper.Fill("CONNECTION_STRING", "SELECT * FROM myTable AS A INNER JOIN secondTable AS B ON A.secondTableId = B.Id;");

///Suprimindo connectionString
var dataTable = MySqlWrapper.Fill("SELECT * FROM myTable AS A INNER JOIN secondTable AS B ON A.secondTableId = B.Id;");

//Variação assíncrona:
var dataTable = await MySqlWrapper.FillAsync("CONNECTION_STRING", "SELECT * FROM myTable AS A INNER JOIN secondTable AS B ON A.secondTableId = B.Id;");
var dataTable = await MySqlWrapper.FillAsync("SELECT * FROM myTable AS A INNER JOIN secondTable AS B ON A.secondTableId = B.Id;");
```

</details>

--------------------------------------------------------------------------------------------------  

# Contribuição

Caso veja algum ponto de melhoria, crítica construtivas são extremamente bem-vindas. Fique à vontade para abrir uma issue e/ou branch para sugerir melhorias.

--------------------------------------------------------------------------------------------------  

# Licença

Licença MIT

Direitos autorais (c) 2024 - Renato Bortolazzi Junior

É concedida permissão, gratuitamente, a qualquer pessoa que obtenha uma cópia
deste software e arquivos de documentação associados (o "Software"), para lidar
no Software sem restrições, incluindo, sem limitação, os direitos
usar, copiar, modificar, mesclar, publicar, distribuir, sublicenciar e/ou vender
cópias do Software e permitir que as pessoas a quem o Software é
capacitado para fazê-lo, sujeito às seguintes condições:

O aviso de direitos autorais acima e este aviso de permissão serão incluídos em todos
cópias ou partes substanciais do Software.

O SOFTWARE É FORNECIDO "COMO ESTÁ", SEM GARANTIA DE QUALQUER TIPO, EXPRESSA OU
IMPLÍCITAS, INCLUINDO, MAS NÃO SE LIMITANDO ÀS GARANTIAS DE COMERCIALIZAÇÃO,
ADEQUAÇÃO A UM DETERMINADO FIM E NÃO VIOLAÇÃO. EM HIPÓTESE ALGUMA O
OS AUTORES OU DETENTORES DE DIREITOS AUTORAIS SERÃO RESPONSÁVEIS POR QUALQUER RECLAMAÇÃO, DANOS OU OUTROS
RESPONSABILIDADE, SEJA EM UMA AÇÃO DE CONTRATO, ATO ILÍCITO OU DE OUTRA FORMA, DECORRENTE DE,
FORA DE OU EM CONEXÃO COM O SOFTWARE OU O USO OU OUTRAS NEGOCIAÇÕES NO
PROGRAMAS.

----

MIT License

Copyright (c) 2024 - Renato Bortolazzi Junior

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
