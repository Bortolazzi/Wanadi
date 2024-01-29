using Microsoft.EntityFrameworkCore;
using Wanadi.MySql.Wrappers;

namespace Wanadi.MySql.Examples.Data;

public static class ContextWrapper
{
    private static string SERVER = "IP_ADDRESS";
    private static string DATABASE = "DATABASE_NAME";
    private static string USER = "USER_MYSQL";
    private static string PASSWORD = "PASSWORD_MYSQL";

    public static string ConnectionString => MySqlWrapper.BuildConnectionString(SERVER, DATABASE, USER, PASSWORD);

    public static WanadiContext Wanadi()
    {
        var optionsBuilder = new DbContextOptionsBuilder<WanadiContext>();
        optionsBuilder.UseMySQL(ConnectionString);

        var resultMethod = new WanadiContext(optionsBuilder.Options);
        
        return resultMethod;
    }
}