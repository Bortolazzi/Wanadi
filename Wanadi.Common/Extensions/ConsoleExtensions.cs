namespace Wanadi.Common.Extensions;

public static class ConsoleExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Imprime no console um texto com a fonte na cor branca.
    ///     </para>
    ///     <para>
    ///         en-US: Prints text on the console with a white font.
    ///     </para>
    /// </summary>
    /// <param name="textInfo">
    ///     <para>
    ///         pt-BR: Texto a ser impresso no console.
    ///     </para>
    ///     <para>
    ///         en-US: Text to be printed on the console.
    ///     </para>
    /// </param>
    /// <param name="printDate">
    ///     <para>
    ///         pt-BR: Se true irá concatenar no começo a data e hora atual seguido de hífen.
    ///     </para>
    ///     <para>
    ///         en-US: If true, it will concatenate the current date and time at the beginning followed by a hyphen.
    ///     </para>
    /// </param>
    public static void PrintInfo(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.White);

    /// <summary>
    ///     <para>
    ///         pt-BR: Imprime no console um texto com a fonte na cor amarela.
    ///     </para>
    ///     <para>
    ///         en-US: Prints text on the console with a yellow font.
    ///     </para>
    /// </summary>
    /// <param name="textInfo">
    ///     <para>
    ///         pt-BR: Texto a ser impresso no console.
    ///     </para>
    ///     <para>
    ///         en-US: Text to be printed on the console.
    ///     </para>
    /// </param>
    /// <param name="printDate">
    ///     <para>
    ///         pt-BR: Se true irá concatenar no começo a data e hora atual seguido de hífen.
    ///     </para>
    ///     <para>
    ///         en-US: If true, it will concatenate the current date and time at the beginning followed by a hyphen.
    ///     </para>
    /// </param>
    public static void PrintWarning(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Yellow);

    /// <summary>
    ///     <para>
    ///         pt-BR: Imprime no console um texto com a fonte na cor vermelha.
    ///     </para>
    ///     <para>
    ///         en-US: Prints text on the console with a red font.
    ///     </para>
    /// </summary>
    /// <param name="textInfo">
    ///     <para>
    ///         pt-BR: Texto a ser impresso no console.
    ///     </para>
    ///     <para>
    ///         en-US: Text to be printed on the console.
    ///     </para>
    /// </param>
    /// <param name="printDate">
    ///     <para>
    ///         pt-BR: Se true irá concatenar no começo a data e hora atual seguido de hífen.
    ///     </para>
    ///     <para>
    ///         en-US: If true, it will concatenate the current date and time at the beginning followed by a hyphen.
    ///     </para>
    /// </param>
    public static void PrintDanger(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Red);

    /// <summary>
    ///     <para>
    ///         pt-BR: Imprime no console um texto com a fonte na cor verde.
    ///     </para>
    ///     <para>
    ///         en-US: Prints text on the console with a green font.
    ///     </para>
    /// </summary>
    /// <param name="textInfo">
    ///     <para>
    ///         pt-BR: Texto a ser impresso no console.
    ///     </para>
    ///     <para>
    ///         en-US: Text to be printed on the console.
    ///     </para>
    /// </param>
    /// <param name="printDate">
    ///     <para>
    ///         pt-BR: Se true irá concatenar no começo a data e hora atual seguido de hífen.
    ///     </para>
    ///     <para>
    ///         en-US: If true, it will concatenate the current date and time at the beginning followed by a hyphen.
    ///     </para>
    /// </param>
    public static void PrintSuccess(this string textInfo, bool printDate = true)
        => PrintInConsole(textInfo, printDate, ConsoleColor.Green);

    /// <summary>
    ///     <para>
    ///         pt-BR: Imprime no console um texto com a fonte na cor vermelha seguido dos detalhes da exceção.
    ///     </para>
    ///     <para>
    ///         en-US: Prints text on the console with a red font followed by the exception details.
    ///     </para>
    /// </summary>
    /// <param name="textInfo">
    ///     <para>
    ///         pt-BR: Texto a ser impresso no console.
    ///     </para>
    ///     <para>
    ///         en-US: Text to be printed on the console.
    ///     </para>
    /// </param>
    /// <param name="exception">
    ///     <para>
    ///         pt-BR: Exceção a ser detalhada no console.
    ///     </para>
    ///     <para>
    ///         en-US: Exception to be detailed in the console.
    ///     </para>
    /// </param>
    /// <param name="printDate">
    ///     <para>
    ///         pt-BR: Se true irá concatenar no começo a data e hora atual seguido de hífen.
    ///     </para>
    ///     <para>
    ///         en-US: If true, it will concatenate the current date and time at the beginning followed by a hyphen.
    ///     </para>
    /// </param>
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