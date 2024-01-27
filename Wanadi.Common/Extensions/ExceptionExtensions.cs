namespace Wanadi.Common.Extensions;

public static class ExceptionExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Transforma as principais propriedades de uma Exception em string e retorna concatenada.
    ///     </para>
    ///     <para>
    ///         en-US: Transforms the main properties of an Exception into Text and returns concatenated.
    ///     </para>
    /// </summary>
    /// <param name="exception">
    ///     <para>
    ///         pt-BR: Exception a ser transformada em string.
    ///     </para>
    ///     <para>
    ///         en-US: Exception to be transformed into a string.
    ///     </para>
    /// </param>
    /// <param name="recursive">
    ///     <para>
    ///         pt-BR: Se true irá recuperar as informações na InnerException. Se false irá ignorar.
    ///     </para>
    ///     <para>
    ///         en-US: If true it will retrieve the information in the InnerException. If false it will ignore.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: String contendo Message, Source, HelpLink, TargetSite, StackTrace, Data e InnerException.
    ///     </para>
    ///     <para>
    ///         en-US: String containing Message, Source, HelpLink, TargetSite, StackTrace, Data and InnerException.
    ///     </para>
    /// </returns>
    public static string ToText(this Exception exception, bool recursive = true)
        => ToText(exception, new StringBuilder(), recursive).ToString();

    private static StringBuilder ToText(Exception exception, StringBuilder errorText, bool recursive)
    {
        if (!string.IsNullOrEmpty(exception.Message))
            errorText.AppendLine().AppendLine($"Exception Message: {exception.Message}");

        if (!string.IsNullOrEmpty(exception.Source))
            errorText.AppendLine().AppendLine($"Exception Source: {exception.Source}");

        if (!string.IsNullOrEmpty(exception.HelpLink))
            errorText.AppendLine().AppendLine($"Exception HelpLink: {exception.HelpLink}");

        if (exception.TargetSite != null)
            errorText.AppendLine().AppendLine($"Exception TargetSite: {exception.TargetSite}");

        if (!string.IsNullOrEmpty(exception.StackTrace))
            errorText.AppendLine().AppendLine($"Exception StackTrace: {exception.StackTrace}");

        if (exception.Data != null && exception.Data.Count > 0)
        {
            errorText.AppendLine().AppendLine($"Exception Data:");
            foreach (var item in exception.Data.Keys)
                errorText.AppendLine($"{item}: {exception.Data[item]}");
        }

        if (recursive && exception.InnerException != null)
        {
            errorText.AppendLine();
            errorText.AppendLine($"Inner Exception:");
            errorText.AppendLine();

            ToText(exception.InnerException, errorText, recursive);
        }

        return errorText;
    }
}