using CreditRisk.API.FinanceEngine.Calculators;
using CreditRisk.API.FinanceEngine.Models;

namespace CreditRisk.API.FinanceEngine;

public class FinanceEngineService
{
    public SharpeResult CalculateSharpe(SharpeInput input)
        => SharpeCalculator.Calculate(input);

    public CapmResult CalculateCAPM(CapmInput input)
        => CapmCalculator.Calculate(input);

    public KellyResult CalculateKelly(KellyInput input)
        => KellyCalculator.Calculate(input);

    public VarResult CalculateVaR(VarInput input)
        => VarCalculator.Calculate(input);

    public GbmResult SimulateGBM(GbmInput input)
        => GbmCalculator.Calculate(input);

    public BlackScholesResult CalculateBlackScholes(BlackScholesInput input)
        => BlackScholesCalculator.Calculate(input);

    public MeanVarianceResult OptimizePortfolio(MeanVarianceInput input)
        => MeanVarianceCalculator.Calculate(input);

    public MertonResult CalculateMerton(MertonInput input)
        => MertonCalculator.Calculate(input);
    // ── Mutual Fund & SIP methods ─────────────────────────────────────────────

    public SipResult CalculateSIP(SipInput input)
        => SipCalculator.Calculate(input);

    public CagrResult CalculateCAGR(CagrInput input)
        => CagrCalculator.Calculate(input);

    public NavResult TrackNAV(NavInput input)
        => NavCalculator.Calculate(input);

    public SipVsLoanResult CompareSipVsLoan(SipVsLoanInput input)
        => SipVsLoanCalculator.Calculate(input);

    // ── Composite method ──────────────────────────────────────────────────
    // Runs all relevant models for one loan and returns a unified risk picture.
    // This is the single endpoint your loan decision screen will call.
    public LoanRiskProfile BuildLoanRiskProfile(LoanRiskProfileRequest request)
    {
        var sharpe = CalculateSharpe(new SharpeInput(
            request.InterestRate,
            request.RiskFreeRate,
            request.ReturnVolatility));

        var capm = CalculateCAPM(new CapmInput(
            request.RiskFreeRate,
            request.MarketReturn,
            request.Beta));

        var kelly = CalculateKelly(new KellyInput(
            1 - request.DefaultProbability,
            request.InterestRate * request.LoanAmount,
            request.LoanAmount * request.DefaultProbability));

        var var30 = CalculateVaR(new VarInput(
            request.LoanAmount,
            request.ReturnVolatility,
            30,
            0.95));

        var merton = CalculateMerton(new MertonInput(
            request.ApplicantAssets,
            request.LoanAmount,
            request.LoanTermYears,
            request.RiskFreeRate,
            request.AssetVolatility));

        double compositeScore = ComputeCompositeScore(sharpe, capm, kelly, merton);

        return new LoanRiskProfile(
            compositeScore,
            sharpe,
            capm,
            kelly,
            var30,
            merton,
            DetermineDecision(compositeScore));
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static double ComputeCompositeScore(
        SharpeResult s, CapmResult c, KellyResult k, MertonResult m)
    {
        // Normalise each model signal to 0–100 scale, then weighted average.
        // Weights reflect how much each model contributes to the final decision.
        double sharpeScore = Math.Clamp(s.SharpeRatio / 3.0 * 100, 0, 100);
        double kellyScore = k.KellyFraction * 100;
        double mertonScore = Math.Clamp(m.DistanceToDefault / 5.0 * 100, 0, 100);
        double capmScore = c.ExpectedReturn <= 0.02 ? 20 : 80;

        return Math.Round(
            0.35 * sharpeScore +
            0.30 * mertonScore +
            0.25 * kellyScore +
            0.10 * capmScore, 2);
    }

    private static string DetermineDecision(double score) => score switch
    {
        >= 70 => "Approve",
        >= 40 => "Review",
        _ => "Reject"
    };
}