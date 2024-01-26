namespace Wanadi.Common.Extensions;

public static class ExceptionExtensions
{
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