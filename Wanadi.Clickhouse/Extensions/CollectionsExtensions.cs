using System.Data;
using Wanadi.Clickhouse.Contracts;

namespace Wanadi.Clickhouse.Extensions;

public static class CollectionsExtensions
{
    public static DataTable ToDataTable<TType>(this List<TType> source, List<PropertyDataWrapper> properties)
    {
        var response = new DataTable();

        foreach (var property in properties)
            response.Columns.Add(property.ColumnName, property.PropertyType);

        foreach (var item in source)
        {
            var row = response.NewRow();

            foreach (var property in properties)
            {
                var value = property.OriginalPropertyInfo.GetValue(item);
                row[property.ColumnName] = value ?? DBNull.Value;
            }

            response.Rows.Add(row);
        }

        return response;
    }
}
