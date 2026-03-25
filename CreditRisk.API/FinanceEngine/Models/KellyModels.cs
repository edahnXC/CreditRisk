using System.Transactions;

namespace CreditRisk.API.FinanceEngine.Models
{
    public record KellyInput(
        double WinProbability,
        double WinAmount,
        double LossAmount
        );
    public record KellyResult(
        double KellyFraction,
        double HalfKelly,
        string Recommendation,
        string Warning
        );
}