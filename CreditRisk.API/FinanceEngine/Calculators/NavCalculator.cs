using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class NavCalculator
{
    public static NavResult Calculate(NavInput input)
    {
        if (input.InitialNAV <= 0) throw new ArgumentException("Initial NAV must be positive.");
        if (input.AnnualVolatility <= 0) throw new ArgumentException("Volatility must be positive.");
        if (input.Years <= 0) throw new ArgumentException("Years must be positive.");

        var rng = new Random(42);
        int timeSteps = input.Years * 12;      // monthly steps
        double dt = 1.0 / 12.0;            // one month as fraction of year
        double mu = input.AnnualReturnRate;
        double sigma = input.AnnualVolatility;

        var allFinalNAVs = new List<double>();
        var samplePaths = new List<double[]>();
        int lossCount = 0;

        for (int sim = 0; sim < input.NumSimulations; sim++)
        {
            double[] path = new double[timeSteps + 1];
            path[0] = input.InitialNAV;

            for (int t = 1; t <= timeSteps; t++)
            {
                // Box-Muller for standard normal random number
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1))
                            * Math.Cos(2.0 * Math.PI * u2);

                // GBM step — same math as income simulation but applied to NAV
                path[t] = path[t - 1] * Math.Exp(
                    (mu - 0.5 * sigma * sigma) * dt
                    + sigma * Math.Sqrt(dt) * z);
            }

            double finalNav = path[timeSteps];
            allFinalNAVs.Add(finalNav);

            if (finalNav < input.InitialNAV) lossCount++;
            if (sim < 20) samplePaths.Add(path);
        }

        // Sort to get percentiles
        allFinalNAVs.Sort();
        int count = allFinalNAVs.Count;
        double medianNAV = allFinalNAVs[count / 2];
        double bullNAV = allFinalNAVs[(int)(count * 0.90)];
        double bearNAV = allFinalNAVs[(int)(count * 0.10)];
        double lossProb = Math.Round((double)lossCount / input.NumSimulations, 4);

        string interpretation =
            $"Over {input.Years} years starting at NAV ₹{input.InitialNAV:F2}: " +
            $"Expected NAV ₹{medianNAV:F2} (median), " +
            $"Bull case ₹{bullNAV:F2} (90th pct), " +
            $"Bear case ₹{bearNAV:F2} (10th pct). " +
            $"Probability of NAV falling below entry: {lossProb:P1}.";

        return new NavResult(
            input.InitialNAV,
            Math.Round(medianNAV, 2),
            Math.Round(bullNAV, 2),
            Math.Round(bearNAV, 2),
            lossProb,
            samplePaths,
            interpretation);
    }
}