using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine.Calculators;

public static class  VarCalculator
{
    private static double GetZScore(double confidence) => confidence switch
    {
        >= 0.99 => 2.326,
        >= 0.975 => 1.960,
        >= 0.95 => 1.645,
        >= 0.90 => 1.282,
        _ => throw new ArgumentException($"Unsupported confidence level: {confidence}")
    };
    public static VarResult Calculate(VarInput input)
    {
        double z = GetZScore(input.ConfidenceLevel);
        double scaledVol=input.AnnualVolatility * Math.Sqrt(input.HorizonDays / 252.0);
        double varAmount=Math.Round(input.LoanAmount * z * scaledVol, 2);
        double varPercent=Math.Round(z * scaledVol * 100, 2);

        string interpretation=
            $"With {input.ConfidenceLevel:P0} confidence, losses over " +
            $"{input.HorizonDays} days will not exceed ₹{varAmount:N0} " +
            $"({varPercent:F1}% of loan value). " +
            (varPercent > 15
                ? "High — review loan terms or collateral requirements."
                : "Within acceptable risk bands.");
        
        return new VarResult(
            varAmount, varPercent,
            input.ConfidenceLevel, input.HorizonDays,
            interpretation);

    }
}