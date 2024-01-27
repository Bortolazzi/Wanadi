namespace Wanadi.Common.Helpers;

public static class MathHelper
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Calcula a quantidade de iterações necessárias para percorrer todo um range.
    ///     </para>
    ///     <para>
    ///         en-US: Calculates the number of iterations needed to traverse an entire range.
    ///     </para>
    /// </summary>
    /// <param name="total">
    ///     <para>
    ///         pt-BR: Quantidade total.
    ///     </para>
    ///     <para>
    ///         en-US: Total amount.
    ///     </para>
    /// </param>
    /// <param name="quantity">
    ///     <para>
    ///         pt-BR: Quantidade por lote.
    ///     </para>
    ///     <para>
    ///         en-US: Quantity per batch.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Quantidade de iterações.
    ///     </para>
    ///     <para>
    ///         en-US: Number of iterations.
    ///     </para>
    /// </returns>
    public static int CalculateIterations(int total, int quantity)
    {
        int iterations = total / quantity;
        if ((total % quantity) > 0)
            iterations++;

        return iterations;
    }
}