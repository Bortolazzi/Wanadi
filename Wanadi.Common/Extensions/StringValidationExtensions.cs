namespace Wanadi.Common.Extensions;

public static  class StringValidationExtensions
{
    public static bool IsCPF(this string strCPF)
    {
        try
        {
            if (string.IsNullOrEmpty(strCPF))
                return false;

            strCPF = strCPF.RemoveNotNumeric().Trim();

            //Caso a string seja diferente de 11 está errado
            if (strCPF.Length != 11)
                return false;

            //Caso a string seja uma cadeia de caracteres repetidos
            if (strCPF.Replace(strCPF[0], ' ').Trim().Length == 0)
                return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = strCPF.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();

            tempCpf = tempCpf + digito;

            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return strCPF.EndsWith(digito);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsCNPJ(this string strCNPJ)
    {
        if (string.IsNullOrEmpty(strCNPJ))
            return false;

        strCNPJ = strCNPJ.RemoveNotNumeric();

        if (strCNPJ.Length != 14)
            return false;

        if (strCNPJ.Replace("0", "").Trim().Length == 0)
            return false;

        int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int soma = 0;
        string tempCnpj = strCNPJ.Substring(0, 12);

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

        int resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        string digito = resto.ToString();

        tempCnpj = tempCnpj + digito;
        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = (soma % 11);
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();

        return strCNPJ.EndsWith(digito);
    }
}