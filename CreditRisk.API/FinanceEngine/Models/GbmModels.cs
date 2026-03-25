namespace CreditRisk.API.FinanceEngine.Models;

public record GbmInput(
    double InitialValue,
    double Drift,
    double Volatility,
    double MinimumThreshold,
    int TimeSteps,
    int NumSimulations
);

public record GbmResult(
    double StressProbability,
    List<double[]> SamplePaths,
    string Interpretation
);