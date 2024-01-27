namespace Wanadi.Common.Attributes;

/// <summary>
/// <para>
///     pt-BR: Atributo para propriedade decimal. DaraWrapper identifica e realiza o round para não acontecer o truncate dos dados.
/// </para>
/// <para>
///     en-US: Attribute for decimal property. DaraWrapper identifies and performs the round so that data is not truncated.
/// </para>
/// <para>
///     Exemplo/Example:
/// </para>
/// <code>
///     [DecimalPrecision(18,2)]
///     public decimal Value { get; set; }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DecimalPrecisionAttribute : Attribute
{
    public DecimalPrecisionAttribute(int precision, int scale)
    {
        Precision = precision;
        Scale = scale;
    }

    public int Precision { get; set; }
    public int Scale { get; set; }
}