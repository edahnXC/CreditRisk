using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class CagrCalculator
{
    public static CagrResult Calculate(CagrInput input)
    {
        if (input.InitialValue <= 0)
            throw new ArgumentException("Initial value must be positive.");
        if (input.FinalValue <= 0)
            throw new ArgumentException("Final value must be positive.");
        if (input.Years <= 0)
            throw new ArgumentException("Years must be positive.");

        // CAGR = (FinalValue / InitialValue) ^ (1 / Years) - 1
        double cagr = Math.Round(
            Math.Pow(input.FinalValue / input.InitialValue, 1.0 / input.Years) - 1, 4);

        double totalGain = Math.Round(input.FinalValue - input.InitialValue, 2);
        double absoluteReturn = Math.Round(totalGain / input.InitialValue * 100, 2);

        // Performance benchmarking against typical market returns
        string performance = cagr switch
        {
            > 0.18 => "Exceptional — significantly above market average.",
            > 0.12 => "Excellent — above typical equity mutual fund returns.",
            > 0.08 => "Good — in line with large-cap index fund returns.",
            > 0.04 => "Average — comparable to debt fund or FD returns.",
            > 0 => "Poor — barely beating inflation.",
            _ => "Negative — capital has eroded over this period."
        };

        string interpretation =
            $"₹{input.InitialValue:N0} grew to ₹{input.FinalValue:N0} " +
            $"over {input.Years:F1} years. " +
            $"CAGR: {cagr:P2}. Absolute return: {absoluteReturn:F1}%. " +
            performance;

        return new CagrResult(cagr, absoluteReturn, totalGain, performance, interpretation);
    }
}