using System.Collections;
using Wanadi.Clickhouse.Contracts;

namespace Wanadi.Clickhouse.Models;

public abstract class ClickHouseEnumeratorEntity : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        var properties = this.GetType().GetProperties().Select(t => new PropertyDataWrapper(t)).ToList();
        properties = properties.Where(t => !t.IgnoreOnInsert).OrderBy(t => t.ColumnName).ToList();

        for (int i = 0; i < properties.Count; i++)
        {
            yield return properties[i].OriginalPropertyInfo.GetValue(this);
        }
    }
}