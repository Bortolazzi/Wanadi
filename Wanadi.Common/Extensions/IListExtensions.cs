using System.Collections;

namespace Wanadi.Common.Extensions;

public static class IListExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Recupera o nome da tabela (TableAttribute) ou type.Name baseado em uma coleção de dados.
    ///     </para>
    ///     <para>
    ///         en-US: Retrieves the table name (TableAttribute) or type.Name based on a collection of data.
    ///     </para>
    /// </summary>
    /// <param name="list">
    ///     <para>
    ///         pt-BR: Coleção de dados para ser recuperado o nome da tabela.
    ///     </para>
    ///     <para>
    ///         en-US: Collection of data to retrieve the table name.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Se os objetos da coleção de dados contiver o TableAttribute será retornado o valor definido. Caso não, será retornado o type.Name.
    ///     </para>
    ///     <para>
    ///         en-US: If the data collection objects contain the TableAttribute, the defined value will be returned. If not, type.Name will be returned.
    ///     </para>
    /// </returns>
    public static string? GetTableName(this IList list)
    {
        if (list == null)
            return null;

        var objectType = list.GetType().GetGenericArguments()[0];

        return objectType.GetTableName();
    }
}