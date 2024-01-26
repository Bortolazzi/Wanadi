using System.Collections;
using System.Collections.Concurrent;
using Wanadi.Common.Enums;
using Wanadi.Common.Extensions;
using Wanadi.Common.Helpers;
using Wanadi.MySql.Contracts.DataWrapper;

namespace Wanadi.MySql.Wrappers;

public static class DataWrapper
{
    public static GuidConditions GuidOption = GuidConditions.IgnoreOnInsert;
    public static EnumConditions EnumOption = EnumConditions.CastToInt;

    public static InsertCommand? GenerateInsertCommand(object itemInsert, params string[] fieldsIgnore)
    {
        if (itemInsert == null)
            return null;

        return GenerateInsertCommand(itemInsert.GetType().GetTableName(), itemInsert, fieldsIgnore);
    }

    public static InsertCommand? GenerateInsertCommand(string tableName, object itemInsert, params string[] fieldsIgnore)
    {
        if (itemInsert == null)
            return null;

        var properties = GetObjectPropertiesByType(itemInsert.GetType(), fieldsIgnore);
        return new InsertCommand(tableName, $"INSERT INTO `{tableName}` ({(string.Join(",", properties.Select(t => $"`{t.ColumnName}`")))}) VALUES ", itemInsert.GetValuesByProperties(properties));
    }

    public static List<InsertCommand> GenerateInsertCommands(IList sourceItems, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<InsertCommand>();

        var tableName = sourceItems.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        return GenerateInsertCommands(tableName, sourceItems, fieldsIgnore);
    }

    public static List<InsertCommand> GenerateInsertCommands(string tableName, IList sourceItems, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<InsertCommand>();

        var properties = GetObjectPropertiesByType(sourceItems.GetType().GetGenericArguments()[0], fieldsIgnore);

        var prefixCommand = $"INSERT INTO `{tableName}` ({(string.Join(",", properties.Select(t => $"`{t.ColumnName}`")))}) VALUES ";

        sourceItems.AsParallel();

        var response = new List<InsertCommand>();

        foreach (var item in sourceItems)
        {
            response.Add(new InsertCommand(tableName, prefixCommand, item.GetValuesByProperties(properties)));
        }

        return response;
    }

    public static List<InsertCommand> GenerateInsertCommandsParallel(IList sourceItems, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<InsertCommand>();

        var tableName = sourceItems.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        return GenerateInsertCommandsParallel(tableName, sourceItems, fieldsIgnore);
    }

    public static List<InsertCommand> GenerateInsertCommandsParallel(string tableName, IList sourceItems, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<InsertCommand>();

        var properties = GetObjectPropertiesByType(sourceItems.GetType().GetGenericArguments()[0], fieldsIgnore);

        var prefixCommand = $"INSERT INTO `{tableName}` ({(string.Join(",", properties.Select(t => $"`{t.ColumnName}`")))}) VALUES ";

        sourceItems.AsParallel();

        var response = new ConcurrentBag<InsertCommand>();

        var parallelResult = Parallel.ForEach(sourceItems.Cast<object>(), currentItem =>
        {
            response.Add(new InsertCommand(tableName, prefixCommand, currentItem.GetValuesByProperties(properties)));
        });

        while (!parallelResult.IsCompleted)
        {
            continue;
        }

        return response.ToList();
    }

    public static List<BatchCommand> GenerateBatchCommands(IList sourceItems, int quantityPerBatch, bool disableKeyCheks = false, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<BatchCommand>();

        var tableName = sourceItems.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        return GenerateBatchCommands(tableName, sourceItems, quantityPerBatch, disableKeyCheks, fieldsIgnore);
    }

    public static List<BatchCommand> GenerateBatchCommands(string tableName, IList sourceItems, int quantityPerBatch, bool disableKeyCheks = false, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<BatchCommand>();

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var insertCommands = GenerateInsertCommands(tableName, sourceItems, fieldsIgnore);
        return GenerateBatchCommands(insertCommands, quantityPerBatch, disableKeyCheks);
    }

    public static List<BatchCommand> GenerateBatchCommandsParallel(IList sourceItems, int quantityPerBatch, bool disableKeyCheks = false, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<BatchCommand>();

        var tableName = sourceItems.GetTableName();
        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        return GenerateBatchCommandsParallel(tableName, sourceItems, quantityPerBatch, disableKeyCheks, fieldsIgnore);
    }

    public static List<BatchCommand> GenerateBatchCommandsParallel(string tableName, IList sourceItems, int quantityPerBatch, bool disableKeyCheks = false, params string[] fieldsIgnore)
    {
        if (sourceItems == null || sourceItems.Count == 0)
            return new List<BatchCommand>();

        if (string.IsNullOrEmpty(tableName))
            throw new Exception("Unable to identify table name or object name.");

        var insertCommands = GenerateInsertCommandsParallel(tableName, sourceItems, fieldsIgnore);
        return GenerateBatchCommands(insertCommands, quantityPerBatch, disableKeyCheks);
    }

    public static List<BatchCommand> GenerateBatchCommands(List<InsertCommand> source, int quantityPerBatch, bool disableKeyCheks = false)
    {
        var response = new List<BatchCommand>();

        var groupedTables = source.GroupBy(t => t.TableName)
                                  .Select(t => new
                                  {
                                      TableName = t.Key,
                                      RecordsToInsert = t.ToList(),
                                      Count = t.Count()
                                  })
                                  .ToList();

        source.Clear();

        foreach (var groupedTable in groupedTables)
        {
            var batchCount = MathHelper.CalculateIterations(groupedTable.Count, quantityPerBatch);

            for (int i = 0; i < batchCount; i++)
            {
                response.Add(new BatchCommand(groupedTable.RecordsToInsert.Skip(i * quantityPerBatch).Take(quantityPerBatch).ToList(), disableKeyCheks));
            }
        }

        groupedTables.Clear();

        return response;
    }

    private static List<ObjectProperty> GetObjectPropertiesByType(Type objectType, params string[] fieldsIgnore)
    {
        var response = objectType.GetProperties()
                                 .Where(t => !fieldsIgnore.Contains(t.Name))
                                 .Select(t => new ObjectProperty(t, EnumOption, GuidOption))
                                 .Where(t => !t.IgnoreOnInsert)
                                 .OrderBy(t => t.ColumnName)
                                 .ToList();

        return response;
    }

    private static List<ValueProperty> GetValuesByProperties(this object item, List<ObjectProperty> properties)
    {
        var objectType = item.GetType();

        return properties.Select(t => new ValueProperty(objectType, item, t, EnumOption, GuidOption)).ToList();
    }
}