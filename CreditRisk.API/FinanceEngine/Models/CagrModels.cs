namespace CreditRisk.API.FinanceEngine.Models;

public record CagrInput(
    double InitialValue,    // starting investment value
    double FinalValue,      // ending investment value
    double Years            // number of years between the two
);

public record CagrResult(
    double CAGR,                    // the annualised growth rate
    double AbsoluteReturn,          // total % gain over entire period
    double TotalGain,               // FinalValue - InitialValue
    string Performance,             // "Excellent" / "Good" / "Average" / "Poor"
    string Interpretation
);