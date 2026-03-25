namespace CreditRisk.API.FinanceEngine.Models
{
    public record CapmInput(
        double RiskFreeRate,
        double MarketReturn,
        double Beta
        );
    public record CapmResult(
        double ExpectedReturn,
        double MarketPremium,
        double RiskPremium,
        string Interpretation
        );
}
