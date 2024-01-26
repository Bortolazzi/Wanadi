namespace Wanadi.Common.Helpers;

public static class MathHelper
{
    public static int CalculateIterations(int total, int quantity)
    {
        int iterations = total / quantity;
        if ((total % quantity) > 0)
            iterations++;

        return iterations;
    }
}