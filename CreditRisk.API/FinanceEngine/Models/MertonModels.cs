namespace CreditRisk.API.FinanceEngine.Models
{
    public record MertonInput(
        double AssetValue,
        double DebtFaceValue,
        double TimeHorizon,
        double RiskFreeRate,
        double AssetVolatility
        );
    public record MertonResult(
        double DistanceToDefault,
        double DefaultProbability,
        double CreditSpread,
        string RiskBand,
        string Interpretation
        );
}
