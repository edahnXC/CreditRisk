using CreditRisk.API.FinanceEngine.Models;
using System.Transactions;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class KellyCalculator { 
    public static KellyResult Calculate(KellyInput input)
    {
        if(input.LossAmount      <=0) throw new ArgumentException("LossAmount must be positive");

        double lossProbability=1.0 - input.WinProbability;
        double winRatio = input.WinAmount / input.LossAmount;

        //core kelly formula 
        double kelly = (input.WinProbability * winRatio - lossProbability) / winRatio;
        double halfKelly = kelly / 2.0;

        //clamp to sensible bounds - never negative.never over 100%
        kelly = Math.Round(Math.Clamp(kelly, 0, 1), 4);
        halfKelly = Math.Round(Math.Clamp(halfKelly, 0, 1), 4);

        string recommendation=kelly<=0
            ? "Do not lend — expected value is negative after default risk."
            : $"Allocate up to {halfKelly:P0} of available capital (half-Kelly). " +
              $"Full Kelly: {kelly:P0}.";

        string warning = kelly > 0.5
            ? "Full Kelly fraction is high — consider capping at 25–30% regardless."
            : string.Empty;

        return new KellyResult(kelly, halfKelly, recommendation, warning);
    }

}

