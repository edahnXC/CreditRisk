namespace CreditRisk.API.FinanceEngine.Models;

public record NavInput(
    double InitialNAV,          // starting NAV value e.g. 10.00
    double AnnualReturnRate,    // expected annual return e.g. 0.12
    double AnnualVolatility,    // fund volatility e.g. 0.18
    int Years,               // projection horizon
    int NumSimulations       // Monte Carlo paths e.g. 500
);

public record NavResult(
    double InitialNAV,
    double ExpectedFinalNAV,    // median projected NAV
    double BullCaseNAV,         // 90th percentile
    double BearCaseNAV,         // 10th percentile
    double ProbabilityOfLoss,   // % of paths ending below InitialNAV
    List<double[]> SamplePaths, // for Angular chart — 20 sample paths
    string Interpretation
);