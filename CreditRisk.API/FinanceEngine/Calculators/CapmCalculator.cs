using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class CapmCalculator
{
    public static CapmResult Calculate(CapmInput input)
    {
        double marketPremium = input.MarketReturn - input.RiskFreeRate;
        double riskPremium = input.Beta * marketPremium;
        double expectedReturn = Math.Round(input.RiskFreeRate + riskPremium, 4);

        string interpretation =
            $"Given β={input.Beta:F2}, minimum required return is {expectedReturn:P1}. " +
            (input.Beta > 1.2
                ? "High market sensitivity — strongly cyclical borrower."
                : input.Beta < 0.5
                    ? "Low market sensitivity — relatively recession-resistant."
                    : "Moderate market sensitivity.");

        return new CapmResult(
           expectedReturn,
            Math.Round(marketPremium, 4),
            Math.Round(riskPremium, 4),
            interpretation);
    }
}