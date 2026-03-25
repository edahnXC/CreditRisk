using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CreditRisk.API.Data;
using CreditRisk.API.FinanceEngine;
using CreditRisk.API.FinanceEngine.Models;
using CreditRisk.API.Models;

namespace CreditRisk.API.Controllers;

[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase
{
    private readonly FinanceEngineService _engine;
    private readonly AppDbContext _db;

    public FinanceController(FinanceEngineService engine, AppDbContext db)
    {
        _engine = engine;
        _db = db;
    }

    [HttpPost("sharpe")]
    public ActionResult<SharpeResult> Sharpe([FromBody] SharpeInput input)
    {
        try { return Ok(_engine.CalculateSharpe(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("capm")]
    public ActionResult<CapmResult> Capm([FromBody] CapmInput input)
    {
        try { return Ok(_engine.CalculateCAPM(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("kelly")]
    public ActionResult<KellyResult> Kelly([FromBody] KellyInput input)
    {
        try { return Ok(_engine.CalculateKelly(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("var")]
    public ActionResult<VarResult> VaR([FromBody] VarInput input)
    {
        try { return Ok(_engine.CalculateVaR(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("gbm")]
    public ActionResult<GbmResult> Gbm([FromBody] GbmInput input)
    {
        try { return Ok(_engine.SimulateGBM(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("blackscholes")]
    public ActionResult<BlackScholesResult> BlackScholes(
        [FromBody] BlackScholesInput input)
    {
        try { return Ok(_engine.CalculateBlackScholes(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("portfolio")]
    public ActionResult<MeanVarianceResult> Portfolio(
        [FromBody] MeanVarianceInput input)
    {
        try { return Ok(_engine.OptimizePortfolio(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("merton")]
    public ActionResult<MertonResult> Merton([FromBody] MertonInput input)
    {
        try { return Ok(_engine.CalculateMerton(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("sip")]
    public ActionResult<SipResult> SIP([FromBody] SipInput input)
    {
        try { return Ok(_engine.CalculateSIP(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("cagr")]
    public ActionResult<CagrResult> CAGR([FromBody] CagrInput input)
    {
        try { return Ok(_engine.CalculateCAGR(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("nav")]
    public ActionResult<NavResult> NAV([FromBody] NavInput input)
    {
        try { return Ok(_engine.TrackNAV(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("sip-vs-loan")]
    public ActionResult<SipVsLoanResult> SipVsLoan(
        [FromBody] SipVsLoanInput input)
    {
        try { return Ok(_engine.CompareSipVsLoan(input)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    // ── Composite endpoint — logs every analysis to database ──────────────
    [HttpPost("loan-risk-profile")]
    public async Task<ActionResult<LoanRiskProfile>> LoanRiskProfile(
        [FromBody] LoanRiskProfileRequest request)
    {
        try
        {
            var result = _engine.BuildLoanRiskProfile(request);

            // Save to AnalysisLog so admin can see all analyses
            var log = new AnalysisLog
            {
                AnalysisType = "LoanRiskProfile",
                CompositeScore = result.CompositeScore,
                Decision = result.Decision,
                LoanAmount = request.LoanAmount,
                InterestRate = request.InterestRate,
                InputJson = JsonSerializer.Serialize(request),
                ResultJson = JsonSerializer.Serialize(new
                {
                    result.CompositeScore,
                    result.Decision,
                    SharpeRatio = result.Sharpe.SharpeRatio,
                    CAPMReturn = result.Capm.ExpectedReturn,
                    KellyFraction = result.Kelly.KellyFraction,
                    VaRAmount = result.VaR.VaRAmount,
                    MertonDD = result.Merton.DistanceToDefault
                })
            };

            _db.AnalysisLogs.Add(log);
            await _db.SaveChangesAsync();

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}