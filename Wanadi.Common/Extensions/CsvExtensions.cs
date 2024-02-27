using System.Text.RegularExpressions;
using Wanadi.Common.Attributes;
using Wanadi.Common.Helpers;

namespace Wanadi.Common.Extensions;

public static class CsvExtensions
{
    public static string[] SplitCsvFields(this string value, char separator)
           => Regex.Split(value, $"{separator}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

    public static string RemoveCsvQuotes(this string value, char quotes)
        => value.TrimStart(quotes).TrimEnd(quotes);

    public static T? ConvertCsvValueToObject<T>(this string value, char separator, char quotes) where T : class
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var properties = typeof(T).GetProperties()
                                  .Select(t => new
                                  {
                                      PropertInfo = t,
                                      CsvField = t.GetAttribute<CsvFieldAttribute>()
                                  })
                                  .Where(t => t.CsvField != null)
                                  .ToList();

        if (!properties.Any())
            throw new Exception($"Class does not have properties with the CsvFieldAttributes.");

        var fields = value.SplitCsvFields(separator);

        var response = Activator.CreateInstance<T>();

        foreach (var property in properties)
        {
            var valueCsv = fields[property.CsvField.Index];

            if (property.CsvField.RemoveStartAndEndQuotes)
                valueCsv = valueCsv.RemoveCsvQuotes(quotes);

            if (property.CsvField.RemoveDoubleSpacesAndEmptyCharacters)
                valueCsv = valueCsv.ClearEmptyCharacters();

            if (property.CsvField.RemoveNotNumericCharacters)
                valueCsv = valueCsv.RemoveNotNumeric();

            if (property.CsvField.RemoveLeftZerosCharacters)
                valueCsv = valueCsv.RemoveLeftZeros();

            if (property.CsvField.ReplaceEmptyToNull && string.IsNullOrEmpty(valueCsv))
                continue;

            if (property.CsvField.PadLeftLength.HasValue && property.CsvField.PadLeftCharacter.HasValue)
                valueCsv = valueCsv.PadLeft(property.CsvField.PadLeftLength.Value, property.CsvField.PadLeftCharacter.Value);

            if (property.CsvField.ConvertData)
            {
                if (property.CsvField.UseConvertHelper)
                {
                    if (property.CsvField.TypeToConvert == typeof(decimal))
                        property.PropertInfo.SetValue(response, ConvertHelper.ToDecimal(valueCsv, true));
                    else if (property.CsvField.TypeToConvert == typeof(DateTime))
                        property.PropertInfo.SetValue(response, ConvertHelper.ToDateTime(valueCsv));
                    else if (property.CsvField.TypeToConvert == typeof(int))
                        property.PropertInfo.SetValue(response, ConvertHelper.ToInt32(valueCsv));
                    else
                        property.PropertInfo.SetValue(response, Convert.ChangeType(valueCsv, property.CsvField.TypeToConvert));

                    continue;
                }

                property.PropertInfo.SetValue(response, Convert.ChangeType(valueCsv, property.CsvField.TypeToConvert));
                continue;
            }


            property.PropertInfo.SetValue(response, valueCsv);
        }

        return response;
    }
}