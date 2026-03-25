namespace CreditRisk.API.FinanceEngine.Models;

public record SipInput(
    double MonthlyInvestment,   // e.g. 5000 = ₹5,000 per month
    double AnnualReturnRate,    // e.g. 0.12 = 12% per year
    int InvestmentMonths     // e.g. 120 = 10 years
);

public record SipResult(
    double MonthlyInvestment,
    double TotalInvested,       // MonthlyInvestment × InvestmentMonths
    double FutureValue,         // what the corpus grows to
    double WealthGained,        // FutureValue - TotalInvested
    double AbsoluteReturn,      // WealthGained / TotalInvested as percentage
    double CAGR,                // annualised return rate
    List<SipYearlyBreakdown> YearlyBreakdown,
    string Interpretation
);

public record SipYearlyBreakdown(
    int Year,
    double TotalInvested,
    double CorpusValue,
    double GainSoFar
);