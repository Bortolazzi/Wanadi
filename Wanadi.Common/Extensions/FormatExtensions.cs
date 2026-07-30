namespace Wanadi.Common.Extensions;

public static class StringFormatExtensions
{
    public static string ToCnpjFormat(this string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return cnpj;

        cnpj = cnpj.RemoveDocumentMask();

        if (cnpj.Length > 2)
            cnpj = cnpj.Insert(2, ".");
        if (cnpj.Length > 6)
            cnpj = cnpj.Insert(6, ".");
        if (cnpj.Length > 10)
            cnpj = cnpj.Insert(10, "/");
        if (cnpj.Length > 15)
            cnpj = cnpj.Insert(15, "-");

        return cnpj;
    }

    public static string ToCpfFormat(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return cpf;

        cpf = cpf.RemoveDocumentMask();

        if (cpf.Length > 3)
            cpf = cpf.Insert(3, ".");
        if (cpf.Length > 7)
            cpf = cpf.Insert(7, ".");
        if (cpf.Length > 11)
            cpf = cpf.Insert(11, "-");

        return cpf;
    }

    public static string ToFormattedInteger(this long value)
        => value.ToString("N0");

    public static string ToFormattedInteger(this int value)
        => value.ToString("N0");

    public static string ToFormattedMoney(this decimal value)
        => value.ToString("N2");
}