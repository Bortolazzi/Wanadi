using System.ComponentModel;

namespace Wanadi.Common.Extensions;

public static class EnumExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Extensão que recupera um a descrição do enumerador. Através do DescriptionAttribute ou .ToString().
    ///     </para>
    ///     <para>
    ///         en-US: Extension that retrieves a description from the enumerator. Through DescriptionAttribute or .ToString().
    ///     </para>
    ///     <para>
    ///     Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         <![CDATA[ var enumDescription = MyEnum.Enum1.Description(); ]]>
    ///     </code>
    /// </summary>
    /// <param name="enum">
    ///     <para>
    ///         pt-BR: Enumerador que será descrito.
    ///     </para>
    ///     <para>
    ///         en-US: Enumerator that will be described.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Descrição contida no DescriptionAttribute ou caso não possua o enum.ToString();
    ///     </para>
    ///     <para>
    ///         en-US: Description contained in DescriptionAttribute or if it does not have enum.ToString();
    ///     </para>
    /// </returns>
    public static string Description(this Enum @enum)
    {
        var enumType = @enum.GetType();
        if (enumType == null)
            return @enum.ToString();

        var infoEnum = enumType.GetField(@enum.ToString());
        if(infoEnum == null)
            return @enum.ToString();

        DescriptionAttribute[] attributesEnum = (DescriptionAttribute[])infoEnum.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributesEnum != null && attributesEnum.Length > 0)
            return attributesEnum[0].Description;
        else
            return @enum.ToString();
    }
}