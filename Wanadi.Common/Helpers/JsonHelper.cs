using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wanadi.Common.Helpers;

public static class JsonHelper
{
    public static DataTable JsonArrayToDataTable(string json, bool flattenNestedObjects = true)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentNullException(nameof(json));

        var token = JToken.Parse(json);
        if (token is not JArray array)
            throw new Exception("O JSON informado deve ser um array.");

        if (array.Count == 0)
            throw new Exception("O array JSON informado deve possuir ao menos um item.");

        var rows = array.Select(t =>
        {
            var values = new Dictionary<string, JToken?>(StringComparer.OrdinalIgnoreCase);
            AddJsonValues(values, t, null, flattenNestedObjects);
            return values;
        }).ToList();

        var columnNames = rows.SelectMany(t => t.Keys)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();

        var response = new DataTable();
        foreach (var columnName in columnNames)
            response.Columns.Add(columnName, GetJsonColumnType(rows.Select(t => t.GetValueOrDefault(columnName))));

        foreach (var row in rows)
        {
            var dataRow = response.NewRow();

            foreach (DataColumn column in response.Columns)
            {
                if (!row.TryGetValue(column.ColumnName, out var value))
                    continue;

                dataRow[column.ColumnName] = GetJsonCellValue(value, column.DataType) ?? DBNull.Value;
            }

            response.Rows.Add(dataRow);
        }

        return response;
    }

    private static void AddJsonValues(Dictionary<string, JToken?> values, JToken token, string? prefix, bool flattenNestedObjects)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                var columnName = string.IsNullOrEmpty(prefix)
                    ? property.Name
                    : $"{prefix}.{property.Name}";

                if (flattenNestedObjects && property.Value is JObject)
                    AddJsonValues(values, property.Value, columnName, flattenNestedObjects);
                else
                    values[columnName] = property.Value;
            }

            return;
        }

        values[prefix ?? "Value"] = token;
    }

    private static Type GetJsonColumnType(IEnumerable<JToken?> values)
    {
        var tokenTypes = values.Where(t => t != null && t.Type != JTokenType.Null)
                               .Select(t => t!.Type)
                               .Distinct()
                               .ToList();

        if (tokenTypes.Count == 0)
            return typeof(string);

        if (tokenTypes.All(t => t == JTokenType.Integer))
        {
            var integerValues = values.Where(t => t != null && t.Type == JTokenType.Integer);
            return integerValues.All(t => t!.Value<long>() is >= int.MinValue and <= int.MaxValue)
                ? typeof(int)
                : typeof(decimal);
        }

        if (tokenTypes.All(t => t is JTokenType.Integer or JTokenType.Float))
            return typeof(decimal);

        if (tokenTypes.All(t => t == JTokenType.Boolean))
            return typeof(bool);

        if (tokenTypes.All(t => t == JTokenType.Date))
            return typeof(DateTime);

        return typeof(string);
    }

    private static object? GetJsonCellValue(JToken? value, Type columnType)
    {
        if (value == null || value.Type == JTokenType.Null)
            return null;

        if (value is JValue jValue)
        {
            if (columnType == typeof(int))
                return jValue.Value<int>();

            if (columnType == typeof(decimal))
                return jValue.Value<decimal>();

            if (columnType == typeof(bool))
                return jValue.Value<bool>();

            if (columnType == typeof(DateTime))
                return jValue.Value<DateTime>();

            return jValue.Value?.ToString();
        }

        return value.ToString(Formatting.None);
    }
}
