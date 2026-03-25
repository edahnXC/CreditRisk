using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class SharpeCalculator
{
    public static SharpeResult Calculate(SharpeInput input)
    {
        if (input.ReturnVolatility <= 0)
            throw new ArgumentException("Return volatility must be positive.");

        double sharpe = Math.Round(
            (input.PortfolioReturn - input.RiskFreeRate) / input.ReturnVolatility, 4);

        string interpretation = sharpe switch
        {
            > 2.0 => "Exceptional. Rare in practice — verify your inputs.",
            > 1.0 => "Good. This loan compensates well for its risk level.",
            > 0.5 => "Acceptable. Borderline — depends on portfolio context.",
            > 0.0 => "Poor. You earn little extra return for the risk taken.",
            _ => "Negative. This loan's return doesn't beat risk-free assets."
        };

        string quality = sharpe > 1.0 ? "Good"
                       : sharpe > 0.5 ? "Acceptable"
                       : "Poor";

        return new SharpeResult(sharpe, interpretation, quality);
    }
}