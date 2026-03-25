using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class BlackScholesCalculator
{
    //standard normal CDF- probability that a normal variable is <=x
    private static double NormalCDF(double x)
    => 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));

    //Error function - Abramowitz & stegun approximation standard in finance 
    private static double Erf(double x)
    {
        double t = 1.0 / (1.0 + 0.3275911 * Math.Abs(x));
        double p = t * (0.254829592
                 + t * (-0.284496736
                 + t * (1.421413741
                 + t * (-1.453152027
                 + t * 1.061405429))));
        double result = 1.0 - p * Math.Exp(-x * x);
        return x >= 0 ? result : -result;
    }
     // Standard normal PDF
    private static double NormalPDF(double x)
        => Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);

    public static BlackScholesResult Calculate(BlackScholesInput input)
    {
        if (input.T <= 0) throw new ArgumentException("Time to maturity must be positive.");
        if (input.Sigma <= 0) throw new ArgumentException("Volatility must be positive.");

        double S = input.S, K = input.K;
        double T = input.T, r = input.R, sigma = input.Sigma;

        double d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * T)
                    / (sigma * Math.Sqrt(T));
        double d2 = d1 - sigma * Math.Sqrt(T);

        double call = S * NormalCDF(d1) - K * Math.Exp(-r * T) * NormalCDF(d2);
        double put = K * Math.Exp(-r * T) * NormalCDF(-d2) - S * NormalCDF(-d1);

        // Greeks
        double delta = Math.Round(NormalCDF(d1), 4);
        double gamma = Math.Round(NormalPDF(d1) / (S * sigma * Math.Sqrt(T)), 6);
        double vega = Math.Round(S * NormalPDF(d1) * Math.Sqrt(T) / 100, 4);
        double theta = Math.Round(
            (-S * NormalPDF(d1) * sigma / (2 * Math.Sqrt(T))
             - r * K * Math.Exp(-r * T) * NormalCDF(d2)) / 365, 4);

        string interpretation =
            $"Prepayment option value: ₹{call:N0}. " +
            (call / S > 0.05
                ? "Option is in-the-money — prepayment risk is high."
                : "Option is near/out of the money — prepayment risk is low.");

        return new BlackScholesResult(
            Math.Round(call, 2), Math.Round(put, 2),
            delta, gamma, vega, theta, interpretation);
    }
}