using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class SipCalculator
{
    public static SipResult Calculate(SipInput input)
    {
        if (input.MonthlyInvestment <= 0)
            throw new ArgumentException("Monthly investment must be positive.");
        if (input.AnnualReturnRate <= 0)
            throw new ArgumentException("Annual return rate must be positive.");
        if (input.InvestmentMonths <= 0)
            throw new ArgumentException("Investment months must be positive.");

        double monthlyRate = input.AnnualReturnRate / 12.0;
        int n = input.InvestmentMonths;
        double P = input.MonthlyInvestment;

        // Standard SIP future value formula
        // FV = P × [((1 + r)^n - 1) / r] × (1 + r)
        double futureValue = P * ((Math.Pow(1 + monthlyRate, n) - 1) / monthlyRate)
                              * (1 + monthlyRate);

        double totalInvested = Math.Round(P * n, 2);
        double fv = Math.Round(futureValue, 2);
        double wealthGained = Math.Round(fv - totalInvested, 2);
        double absoluteReturn = Math.Round(wealthGained / totalInvested * 100, 2);

        // CAGR of the SIP corpus
        int years = n / 12;
        double cagr = years > 0
            ? Math.Round(Math.Pow(fv / totalInvested, 1.0 / years) - 1, 4)
            : 0;

        // Yearly breakdown — useful for Angular chart showing growth over time
        var breakdown = new List<SipYearlyBreakdown>();
        for (int y = 1; y <= Math.Max(years, 1); y++)
        {
            int months = y * 12;
            double corpusAtY = P
                * ((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate)
                * (1 + monthlyRate);
            double investedAtY = P * months;

            breakdown.Add(new SipYearlyBreakdown(
                y,
                Math.Round(investedAtY, 2),
                Math.Round(corpusAtY, 2),
                Math.Round(corpusAtY - investedAtY, 2)));
        }

        string interpretation =
            $"Investing ₹{P:N0}/month for {years} years at {input.AnnualReturnRate:P1} " +
            $"grows ₹{totalInvested:N0} invested into ₹{fv:N0}. " +
            $"Wealth gained: ₹{wealthGained:N0} ({absoluteReturn:F1}% absolute return). " +
            $"Effective CAGR: {cagr:P2}.";

        return new SipResult(
            P, totalInvested, fv, wealthGained,
            absoluteReturn, cagr, breakdown, interpretation);
    }
}