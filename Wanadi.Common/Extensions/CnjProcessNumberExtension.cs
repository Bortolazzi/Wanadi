namespace Wanadi.Common.Extensions;

public static class CnjProcessNumberExtension
{
    public static bool IsValidCnjProcessNumber(this string? cnj)
    {
        var cleaned = cnj.RemoveNotNumeric();

        if (cleaned.Length != 20)
            return false;

        if (!int.TryParse(cleaned[..7], out var sequentialNumber))
            return false;

        var operationDv = sequentialNumber % 97;

        if (!long.TryParse($"{operationDv}{cleaned.Substring(9, 4)}{cleaned.Substring(13, 1)}{cleaned.Substring(14, 2)}", out var firstOperationValue))
            return false;

        var operation1 = firstOperationValue % 97;

        if (!long.TryParse($"{operation1}{cleaned.Substring(16, 4)}{cleaned.Substring(7, 2)}", out var secondOperationValue))
            return false;

        var operation2 = secondOperationValue % 97;

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
