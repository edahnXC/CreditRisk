namespace CreditRisk.API.FinanceEngine.Models;

public record SipVsLoanInput(
    // Loan side
    double LoanInterestRate,        // e.g. 0.09 = 9%
    double LoanDefaultProbability,  // e.g. 0.08 = 8% chance of default
    double LoanVolatility,          // return volatility of loan portfolio

    // SIP / Mutual Fund side
    double FundAnnualReturn,        // e.g. 0.12 = 12% expected return
    double FundVolatility,          // e.g. 0.18 = 18% annual volatility

    // Common
    double RiskFreeRate,            // e.g. 0.04 = 4%
    double Capital,                 // total capital to deploy e.g. 500000
    int Years                    // investment horizon
);

public record SipVsLoanResult(
    // Loan metrics
    double LoanEffectiveReturn,
    double LoanSharpeRatio,
    double LoanFutureValue,
    string LoanRating,

    // SIP / Fund metrics
    double FundEffectiveReturn,
    double FundSharpeRatio,
    double FundFutureValue,
    string FundRating,

    // Verdict
    string Recommendation,
    string ReasonSummary,
    double SharpeAdvantage     // positive = fund wins, negative = loan wins
);