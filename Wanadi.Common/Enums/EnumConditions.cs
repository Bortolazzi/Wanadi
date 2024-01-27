namespace Wanadi.Common.Enums;

public enum EnumConditions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Ignora propriedades do tipo Enum ao criar o script de inserção.
    ///     </para>
    ///     <para>
    ///         en-US: Ignores Enum type properties when creating the insert script.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         DataWrapper.EnumOption = EnumConditions.IgnoreOnInsert;
    ///     </code>
    /// </summary>
    IgnoreOnInsert,

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a conversão do valor das propriedades do tipo Enum em string ao criar o script de inserção.
    ///     </para>
    ///     <para>
    ///         Recupera o valor contido no DescriptionAttribute. Caso não tenha irá capturar o nome do enumerador. (ex: "CastToString")
    ///     </para>
    ///     <para>
    ///         en-US: Performs the conversion of the value of Enum type properties to string when creating the insertion script.
    ///     </para>
    ///     <para>
    ///         Get the value contained in the DescriptionAttribute. If not, it will capture the name of the enumerator. (ex: "CastToString")
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         DataWrapper.EnumOption = EnumConditions.CastToString;
    ///     </code>
    /// </summary>
    CastToString,

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a conversão do valor das propriedades do tipo Enum em int ao criar o script de inserção.
    ///     </para>
    ///     <para>
    ///         Recupera o valor inteiro do enumerador. (Ex: (int)EnumConditions.CastToInt)
    ///     </para>
    ///     <para>
    ///         en-US: Performs the conversion of the value of Enum type properties to int when creating the insertion script.
    ///     </para>
    ///     <para>
    ///         Get the int value of the enumerator. (Ex: (int)EnumConditions.CastToInt)
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         DataWrapper.EnumOption = EnumConditions.CastToInt;
    ///     </code>
    /// </summary>
    CastToInt,
}