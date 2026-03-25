using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class GbmCalculator
{
    public static GbmResult Calculate(GbmInput input)
    {
        var rng = new Random(42); // fixed seed = reproducible results
        int belowThreshold = 0;
        var paths = new List<double[]>();

        for (int sim = 0; sim < input.NumSimulations; sim++)
        {
            double[] path = new double[input.TimeSteps + 1];
            path[0] = input.InitialValue;

            for (int t = 1; t <= input.TimeSteps; t++)
            {
                // Box-Muller transform — generates standard normal random number
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1))
                            * Math.Cos(2.0 * Math.PI * u2);

                double dt = 1.0 / input.TimeSteps;

                // GBM discrete update step
                path[t] = path[t - 1] * Math.Exp(
                    (input.Drift - 0.5 * input.Volatility * input.Volatility) * dt
                    + input.Volatility * Math.Sqrt(dt) * z);
            }

            if (path[input.TimeSteps] < input.MinimumThreshold)
                belowThreshold++;

            // Store first 20 paths only — enough for chart rendering in Angular
            if (sim < 20)
                paths.Add(path);
        }

        double stressProb = Math.Round(
            (double)belowThreshold / input.NumSimulations, 4);

        string interpretation = stressProb > 0.3
            ? $"High stress risk: {stressProb:P0} of simulated paths breach threshold."
            : stressProb > 0.1
                ? $"Moderate stress risk: {stressProb:P0} of paths breach threshold."
                : $"Low stress risk: only {stressProb:P0} of paths breach threshold.";

        return new GbmResult(stressProb, paths, interpretation);
    }
}