namespace Wanadi.Common.Extensions;

public static class ConsoleExtensions
{
    public static void PrintInfo(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.White);

    public static void PrintWarning(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Yellow);

    public static void PrintDanger(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Red);

    public static void PrintSuccess(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Green);

    public static void PrintError(this string textInfo, Exception exception, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Red, exception);

    private static void PrintInConsole(string text, bool printDate, ConsoleColor textoColor, Exception? exception = null)
    {
        Console.ResetColor();
        Console.ForegroundColor = textoColor;

        if (printDate)
            Console.WriteLine($"{DateTime.Now} - {text}");
        else
            Console.WriteLine(text);

        if (exception != null)
        {
            Console.WriteLine();
            Console.WriteLine(exception.ToText(true));
            Console.WriteLine();
        }

        Console.ResetColor();
    }
}