namespace CreditRisk.API.FinanceEngine.Models;

public record MeanVarianceInput(
    double[] ExpectedReturns,
    double[][] CovarianceMatrix,
    double RiskFreeRate,
    double[]? Weights
);

public record MeanVarianceResult(
    double PortfolioReturn,
    double PortfolioVolatility,
    double SharpeRatio,
    double[] Weights,
    List<FrontierPoint> FrontierPoints
);

public record FrontierPoint(
    double Risk,
    double Return
);