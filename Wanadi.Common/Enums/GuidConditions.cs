namespace Wanadi.Common.Enums;

public enum GuidConditions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Ignora propriedades do tipo Guid ao criar o script de inserção.
    ///     </para>
    ///     <para>
    ///         en-US: Ignores Guid type properties when creating the insert script.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         DataWrapper.GuidOption = GuidConditions.IgnoreOnInsert;
    ///     </code>
    /// </summary>
    IgnoreOnInsert,

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a conversão do valor das propriedades do tipo Guid em string ao criar o script de inserção.
    ///     </para>
    ///     <para>
    ///         en-US: Performs the conversion of the value of Guid type properties to string when creating the insertion script.
    ///     </para>
    ///     <para>
    ///         Exemplo/Example:
    ///     </para>
    ///     <code>
    ///         DataWrapper.GuidOption = GuidConditions.CastToString;
    ///     </code>
    /// </summary>
	CastToString
}