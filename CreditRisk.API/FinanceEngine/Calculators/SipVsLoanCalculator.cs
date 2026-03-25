using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class SipVsLoanCalculator
{
    public static SipVsLoanResult Calculate(SipVsLoanInput input)
    {
        // ── Loan side ─────────────────────────────────────────────────────
        // Effective return on loan = interest rate × (1 - default probability)
        // If there is an 8% chance of default, you don't earn the full 9%
        double loanEffectiveReturn = input.LoanInterestRate
                                     * (1 - input.LoanDefaultProbability);

        double loanSharpe = input.LoanVolatility > 0
            ? Math.Round(
                (loanEffectiveReturn - input.RiskFreeRate) / input.LoanVolatility, 4)
            : 0;

        // Future value of capital deployed as loans
        double loanFutureValue = Math.Round(
            input.Capital * Math.Pow(1 + loanEffectiveReturn, input.Years), 2);

        string loanRating = loanSharpe switch
        {
            > 1.5 => "Excellent",
            > 1.0 => "Good",
            > 0.5 => "Acceptable",
            _ => "Poor"
        };

        // ── SIP / Mutual Fund side ────────────────────────────────────────
        double fundSharpe = input.FundVolatility > 0
            ? Math.Round(
                (input.FundAnnualReturn - input.RiskFreeRate) / input.FundVolatility, 4)
            : 0;

        // Future value using standard compound growth (lump sum into fund)
        double fundFutureValue = Math.Round(
            input.Capital * Math.Pow(1 + input.FundAnnualReturn, input.Years), 2);

        string fundRating = fundSharpe switch
        {
            > 1.5 => "Excellent",
            > 1.0 => "Good",
            > 0.5 => "Acceptable",
            _ => "Poor"
        };

        // ── Comparison ────────────────────────────────────────────────────
        double sharpeAdvantage = Math.Round(fundSharpe - loanSharpe, 4);

        string recommendation;
        string reasonSummary;

        if (sharpeAdvantage > 0.2)
        {
            recommendation = "Invest in Mutual Fund";
            reasonSummary =
                $"The fund offers a Sharpe of {fundSharpe:F2} vs loan's {loanSharpe:F2}. " +
                $"Better risk-adjusted return by {sharpeAdvantage:F2} Sharpe points. " +
                $"Fund grows capital to ₹{fundFutureValue:N0} vs loan's ₹{loanFutureValue:N0}.";
        }
        else if (sharpeAdvantage < -0.2)
        {
            recommendation = "Deploy as Loan";
            reasonSummary =
                $"Lending offers a Sharpe of {loanSharpe:F2} vs fund's {fundSharpe:F2}. " +
                $"Loan provides better risk-adjusted return by {Math.Abs(sharpeAdvantage):F2} points. " +
                $"Loan grows capital to ₹{loanFutureValue:N0} vs fund's ₹{fundFutureValue:N0}.";
        }
        else
        {
            recommendation = "Balanced Split — Consider Both";
            reasonSummary =
                $"Sharpe ratios are close (Loan: {loanSharpe:F2}, Fund: {fundSharpe:F2}). " +
                $"Consider splitting capital: 60% loans, 40% fund for diversification. " +
                $"Loan FV: ₹{loanFutureValue:N0}, Fund FV: ₹{fundFutureValue:N0}.";
        }

        return new SipVsLoanResult(
            Math.Round(loanEffectiveReturn, 4), loanSharpe, loanFutureValue, loanRating,
            Math.Round(input.FundAnnualReturn, 4), fundSharpe, fundFutureValue, fundRating,
            recommendation, reasonSummary, sharpeAdvantage);
    }
}