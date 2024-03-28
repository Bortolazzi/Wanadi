using Wanadi.MySql.Examples.Enums;

namespace Wanadi.MySql.Examples;

public static class DataGenerator
{
    public static string? RandomString(int length)
    {
        if (length == 0)
            return null;

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 !@#$%ˆ&*()_+=-[]{};':\"/.,<>?`˜\\";
        var random = new Random();

        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static int RandomInt(int start, int end)
    {
        var random = new Random();
        return random.Next(start, end);
    }

    public static DateTime? RandomDateMonth(int month)
    {
        if (month == 0)
            return null;

        return new DateTime(DateTime.Now.Year, month, 1);
    }

    public static decimal RandomDecimal(int scaleMin, int scaleMax)
    {
        var rng = new Random();

        var precisionSize = rng.Next(2, 16);
        var scaleSize = rng.Next(scaleMin, scaleMax);

        var precision = string.Empty;
        for (int i = 0; i < precisionSize; i++)
            precision += RandomInt(0, 9);

        var scale = string.Empty;
        for (int i = 0; i < scaleSize; i++)
            scale += RandomInt(0, 9);

        var response = Convert.ToDecimal($"{precision},{scale}");

        return response;
    }

    public static Status RandomEnumStatus()
    {
        var random = new Random();
        var value = random.Next(1, 3);
        return (Status)value;
    }
}