
namespace CreditRisk.API.FinanceEngine.Models
{
    public record SharpeInput(
        double PortfolioReturn,
        double RiskFreeRate,
        double ReturnVolatility
        );
    public record SharpeResult(
        double SharpeRatio,
        string Interpretation,
        string Quality
        );
}
