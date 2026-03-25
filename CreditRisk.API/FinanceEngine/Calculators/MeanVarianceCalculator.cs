using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class MeanVarianceCalculator
{
    public static MeanVarianceResult Calculate(MeanVarianceInput input)
    {
        int n = input.ExpectedReturns.Length;
        double[] weights = input.Weights ?? EqualWeights(n);

        // Portfolio return — weighted average of individual loan returns
        double portfolioReturn = 0;
        for (int i = 0; i < n; i++)
            portfolioReturn += weights[i] * input.ExpectedReturns[i];

        // Portfolio variance — captures how loans move together (covariance)
        double portfolioVariance = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                portfolioVariance += weights[i] * weights[j]
                                     * input.CovarianceMatrix[i][j];

        double portfolioVolatility = Math.Sqrt(portfolioVariance);

        double portfolioSharpe = portfolioVolatility > 0
            ? (portfolioReturn - input.RiskFreeRate) / portfolioVolatility
            : 0;

        var frontierPoints = GenerateFrontierPoints(
            input.ExpectedReturns,
            input.CovarianceMatrix,
            input.RiskFreeRate,
            steps: 30);

        return new MeanVarianceResult(
            Math.Round(portfolioReturn, 4),
            Math.Round(portfolioVolatility, 4),
            Math.Round(portfolioSharpe, 4),
            weights,
            frontierPoints);
    }

    private static double[] EqualWeights(int n)
    {
        var w = new double[n];
        for (int i = 0; i < n; i++) w[i] = 1.0 / n;
        return w;
    }

    private static List<FrontierPoint> GenerateFrontierPoints(
        double[] returns, double[][] cov, double rf, int steps)
    {
        var points = new List<FrontierPoint>();
        int n = returns.Length;
        double sumR = returns.Sum();

        for (int step = 0; step <= steps; step++)
        {
            double t = (double)step / steps;
            var w = new double[n];

            for (int i = 0; i < n; i++)
                w[i] = (1 - t) * (1.0 / n) + t * (returns[i] / sumR);

            double ret = 0, variance = 0;
            for (int i = 0; i < n; i++) ret += w[i] * returns[i];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    variance += w[i] * w[j] * cov[i][j];

            points.Add(new FrontierPoint(
                Math.Round(Math.Sqrt(variance), 4),
                Math.Round(ret, 4)));
        }

        return points;
    }
}