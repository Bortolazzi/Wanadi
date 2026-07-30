namespace Wanadi.Common.Extensions;

public static class CnjProcessNumberExtension
{
    public static bool IsValidCnjProcessNumber(this string cnj)
    {
        if (cnj.Length != 20)
            return false;

        var operationDv = int.Parse(cnj[..7]) % 97;
        var operation1 = long.Parse($"{operationDv}{cnj.Substring(9, 4)}{cnj.Substring(13, 1)}{cnj.Substring(14, 2)}") % 97;
        var operation2 = long.Parse($"{operation1}{cnj.Substring(16, 4)}{cnj.Substring(7, 2)}") % 97;

        return operation2 == 1;
    }

    public static string ToProcessNumberFormat(this string processNumber)
    {
        var cleaned = processNumber.RemoveNotNumeric();

        if (cleaned.Length is 19 or 20)
        {
            int sequencialLength = cleaned.Length == 20 ? 7 : 6;

            var sequencial = cleaned.Substring(0, sequencialLength);
            var digito = cleaned.Substring(sequencialLength, 2);
            var ano = cleaned.Substring(sequencialLength + 2, 4);
            var segmento = cleaned.Substring(sequencialLength + 6, 1);
            var tribunal = cleaned.Substring(sequencialLength + 7, 2);
            var orgao = cleaned.Substring(sequencialLength + 9, 4);

            return $"{sequencial}-{digito}.{ano}.{segmento}.{tribunal}.{orgao}";
        }

        return processNumber;
    }
}