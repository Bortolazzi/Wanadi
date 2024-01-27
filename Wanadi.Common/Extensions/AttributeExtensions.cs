using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Wanadi.Common.Extensions;

public static class AttributeExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Extensão que recupera um atributo de uma propriedade.
    ///     </para>
    ///     <para>
    ///         en-US: Extension that retrieves an attribute from a property.
    ///     </para>
    ///     <para>
    ///     Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         <![CDATA[ var columnName = myProperty.GetAttribute<ColumnNameAttribute>(); ]]>
    ///     </code>
    /// </summary>
    /// <typeparam name="T">
    ///     <para>
    ///         pt-BR: Tipo do atributo a ser recuperado da propriedade.
    ///     </para>
    ///     <para>
    ///         en-US: Type of attribute to be retrieved from the property.
    ///     </para>
    /// </typeparam>
    /// <param name="propertyInfo">
    ///     <para>
    ///         pt-BR: Propriedade que irá ser verificada para recuperação do atributo.
    ///     </para>
    ///     <para>
    ///         en-US: Property that will be checked to retrieve the attribute.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Caso encontrado será retornado o objeto contendo as informações do atributo. Caso não encontrado será retornado null.
    ///     </para>
    ///     <para>
    ///         en-US: If found, the object containing the attribute information will be returned. If not found, null will be returned.
    ///     </para>
    /// </returns>
    public static T? GetAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
    {
        var attributeType = typeof(T);

        var customAttribute = propertyInfo.GetCustomAttributes(attributeType, false);
        if (customAttribute == null)
            return null;

        var responseAttribute = customAttribute.FirstOrDefault();
        if (responseAttribute == null)
            return null;

        return (T)responseAttribute;
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Extensão que recupera um atributo de uma tipo.
    ///     </para>
    ///     <para>
    ///         en-US: Extension that retrieves an attribute from a type.
    ///     </para>
    ///     <para>
    ///     Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         <![CDATA[ var tableName = typeof(MyClass).GetAttribute<TableAttribute>(); ]]>
    ///     </code>
    ///     <code>
    ///         <![CDATA[ var anotherExample = myObject.GetType().GetAttribute<TableAttribute>(); ]]>
    ///     </code>
    /// </summary>
    /// <typeparam name="T">
    ///     <para>
    ///         pt-BR: Tipo do atributo a ser recuperado do tipo.
    ///     </para>
    ///     <para>
    ///         en-US: Type of attribute to be retrieved from the type.
    ///     </para>
    /// </typeparam>
    /// <param name="type">
    ///     <para>
    ///         pt-BR: Tipo que será verificado para recuperação do atributo.
    ///     </para>
    ///     <para>
    ///         en-US: Type that will be checked to retrieve the attribute.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Caso encontrado será retornado o objeto contendo as informações do atributo. Caso não encontrado será retornado null.
    ///     </para>
    ///     <para>
    ///         en-US: If found, the object containing the attribute information will be returned. If not found, null will be returned.
    ///     </para>
    /// </returns>
    public static T? GetAttribute<T>(this Type type) where T : Attribute
    {
        var attributeType = typeof(T);

        var customAttribute = type.GetCustomAttributes(attributeType, false);
        if (customAttribute == null)
            return null;

        var responseAttribute = customAttribute.FirstOrDefault();
        if (responseAttribute == null)
            return null;

        return (T)responseAttribute;
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Extensão que recupera o valor contido no TableAttribute ou o Name do tipo informado.
    ///     </para>
    ///     <para>
    ///         en-US: Extension that retrieves the value contained in the TableAttribute or the Name of the type entered.
    ///     </para>
    ///     <para>
    ///     Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         <![CDATA[ var tableName = typeof(MyClass).GetTableName(); ]]>
    ///     </code>
    ///     <code>
    ///         <![CDATA[ var anotherExample = myObject.GetType().GetTableName(); ]]>
    ///     </code>
    /// </summary>
    /// <param name="type">
    ///     <para>
    ///         pt-BR: Tipo que será verificado para recuperação do TableAttribute ou type.Name.
    ///     </para>
    ///     <para>
    ///         en-US: Type that will be checked to retrieve the TableAttribute or type.Name.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Caso encontrado será retornado o valor informado no atributo. Caso não encontrado será retornado o Name do tipo.
    ///     </para>
    ///     <para>
    ///         en-US: If found, the value entered in the attribute will be returned. If not found, the Name of the type will be returned.
    ///     </para>
    /// </returns>
    public static string GetTableName(this Type type)
    {
        var tableNameAttribute = type.GetAttribute<TableAttribute>();
        return tableNameAttribute?.Name ?? type.Name;
    }
}