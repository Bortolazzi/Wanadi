namespace Wanadi.MySql.Contracts;

public class MySqlConnectionSettings
{
    public string Server { get; set; }
    public string UserID { get; set; }
    public string Password { get; set; }
    public uint? CommandTimeout { get; set; } = 180;
    public uint? Port { get; set; } = 3306;
    public uint? MaximumPoolSize { get; set; } = 5000;
    public bool? AllowLoadLocalInfile { get; set; } = true;
}