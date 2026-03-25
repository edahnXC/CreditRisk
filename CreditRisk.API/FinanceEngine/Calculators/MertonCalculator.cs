using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class MertonCalculator
{
    private static double NormalCDF(double x)
        => 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));

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

    public static MertonResult Calculate(MertonInput input)
    {
        double A = input.AssetValue;
        double D = input.DebtFaceValue;
        double T = input.TimeHorizon;
        double r = input.RiskFreeRate;
        double sA = input.AssetVolatility;

        // Distance to default — standard deviations away from insolvency
        double dd = (Math.Log(A / D) + (r - 0.5 * sA * sA) * T)
                    / (sA * Math.Sqrt(T));

        double defaultProbability = NormalCDF(-dd);
        double creditSpread = -Math.Log(1 - defaultProbability) / T;

        string riskBand = dd switch
        {
            > 3.0 => "Safe",
            > 2.0 => "Moderate",
            > 1.0 => "Elevated",
            _ => "Distressed"
        };

        string interpretation =
            $"Distance to default: {dd:F2} std deviations. " +
            $"Default probability over {T:F1} years: {defaultProbability:P1}. " +
            $"Risk band: {riskBand}. " +
            $"Implied credit spread: {creditSpread:P2}.";

        return new MertonResult(
            Math.Round(dd, 4),
            Math.Round(defaultProbability, 4),
            Math.Round(creditSpread, 4),
            riskBand,
            interpretation);
    }
}
