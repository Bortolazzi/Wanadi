using System.Collections;
using Wanadi.Common.Contracts.PropertyMappers;

namespace Wanadi.Clickhouse.Models;

public abstract class ClickHouseEnumeratorEntity : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        var properties = this.GetType().GetProperties().Select(t => new PropertyDataType(t)).ToList();
        properties = properties.Where(t => !t.IgnoreOnInsert).OrderBy(t => t.ColumnName).ToList();

        for (int i = 0; i < properties.Count; i++)
        {
            yield return properties[i].PropertyInfo.GetValue(this);
        }
    }
}