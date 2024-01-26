## Sobre:  

Projeto desenvolvido para encapsular algumas funções que facilitam o dia a dia do desenvolvedor. Estão inclusas algumas funcionalidades básicas como, recuperar atributos de propriedades e objetos, operações em banco de dados MySql, gerar scripts de inserção dinamicamente, independente de ser um objeto fortemente tipado ou um objeto anônimo. 

A ideia é manter o projeto atualizado e crescendo com algumas funcionalidades que irão ajudar no cotidiano.

* **NuGet Wanadi.Common** https://www.nuget.org/packages/Wanadi.Common
    - Disponível para .net6 e .net8
* **NuGet Wanadi.MySql** https://www.nuget.org/packages/Wanadi.MySql
    - Disponível para .net6 e .net8

--------------------------------------------------------------------------------------------------  
## ToDo List:
- [x] Publicar primeira versão dos pacotes
- [ ] Criar summaries no código para facilitar a utilização
- [ ] Adicionar FileHelper (Para leitura e escrita de arquivos)
- [ ] Adicionar CsvHelper (Para ajudar na leitura de arquivos CSV e conversão para objetos tipados)

--------------------------------------------------------------------------------------------------  
> [!WARNING]
> Código está livre para leitura e críticas, atenção ao usá-lo. Fique à vontade para sugerir mudanças, abrir uma branch para ajudar a melhora-lo.

--------------------------------------------------------------------------------------------------  
## Wanadi.MySql - Como usar:

Caso utilize apenas uma string de conexão em seu projeto pode fazer a configuração através de:

* [MySqlWrapper.cs](./Wanadi.MySql/Wrappers/MySqlWrapper.cs)

```csharp
MySqlWrapper.ConnectionString = "CONNECTION_STRING";

//Ou pode gerar através do método BuildConnectionString:

MySqlWrapper.ConnectionString = MySqlWrapper.ConnectionString("MYSQL_SERVER", "DATABASE_NAME", "USER", "PASSWORD");
```

Após star a ConnectionString os métodos de execução de script podem ser utilizados suprimindo a connectionString da chamada.

--------------------------------------------------------------------------------------------------  

## SelectQuery\<TType\>
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

### SelectQueryByEntity\<TType\>
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

### SelectQueryFirstOrDefault\<TType\>
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

### ExecuteNonQuery
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

### ExecuteScalar
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

### Fill
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
