namespace CreditRisk.API.FinanceEngine.Models
{
    public record LoanRiskProfileRequest(
        double LoanAmount,
        double InterestRate,
        double RiskFreeRate,
        double MarketReturn,
        double Beta,
        double ReturnVolatility,
        double DefaultProbability,
        double LoanTermYears,
        double ApplicantAssets,
        double AssetVolatility
        );
    public record LoanRiskProfile(
        double          CompositeScore,
        SharpeResult    Sharpe,
        CapmResult      Capm,
        KellyResult     Kelly,
        VarResult       VaR,
        MertonResult    Merton,
        string          Decision
        );
}
