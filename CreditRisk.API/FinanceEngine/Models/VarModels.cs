namespace CreditRisk.API.FinanceEngine.Models;

public record VarInput(
    double LoanAmount,
    double AnnualVolatility,
    int HorizonDays,
    double ConfidenceLevel
);

public record VarResult(
    double VaRAmount,
    double VaRPercent,
    double ConfidenceLevel,
    int HorizonDays,
    string Interpretation
);