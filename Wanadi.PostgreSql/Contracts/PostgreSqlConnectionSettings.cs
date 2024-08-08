namespace Wanadi.PostgreSql.Contracts;

public class PostgreSqlConnectionSettings
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int? Port { get; set; } = 5432;
    public int? Timeout { get; set; } = 15;
    public int? CommandTimeout { get; set; } = 30;
    public int? CancellationTimeout { get; set; } = 2000;
    public int? MaxPoolSize { get; set; } = 5000;
    public int? KeepAlive { get; set; } = 0;
    public bool? IncludeErrorDetail { get; set; } = false;
}