# Wanadi

Bibliotecas .NET com funcionalidades reutilizáveis para aplicações e acesso a PostgreSQL.

## Pacotes

- [Wanadi.Common](https://www.nuget.org/packages/Wanadi.Common): helpers, extensões, clientes HTTP, leitura de CSV e Excel e utilitários de uso geral.
- [Wanadi.PostgreSql](https://www.nuget.org/packages/Wanadi.PostgreSql): wrappers e repositórios para operações com PostgreSQL e Npgsql.

Os projetos têm como alvo o .NET 10.

## Projetos de exemplo

- `Wanadi.Common.Examples`
- `Wanadi.PostgreSql.Examples`

Os exemplos que acessam PostgreSQL exigem uma instância de banco configurada pelo consumidor.

## Compilação

```bash
dotnet restore Wanadi.slnx
dotnet build Wanadi.slnx --no-restore
```

## Contribuição

Sugestões, críticas e pull requests são bem-vindos.

## Licença

Distribuído sob a licença [MIT](LICENSE).
