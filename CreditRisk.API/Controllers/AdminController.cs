using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreditRisk.API.Data;
using CreditRisk.API.Models;
using CreditRisk.API.Services;

namespace CreditRisk.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MarketDataService _market;

    public AdminController(AppDbContext db, MarketDataService market)
    {
        _db = db;
        _market = market;
    }

    // ── Auth ──────────────────────────────────────────────────────────────
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (request.Password == "CreditRisk@Admin2024")
            return Ok(new LoginResponse(true, "admin-session-token-2024"));
        return Unauthorized(new LoginResponse(false, ""));
    }

    // ── Analytics ─────────────────────────────────────────────────────────
    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsSummary>> GetAnalytics()
    {
        var logs = await _db.AnalysisLogs.ToListAsync();
        var total = logs.Count;
        var avgScore = total > 0 ? logs.Average(l => l.CompositeScore) : 0;
        var approved = logs.Count(l => l.Decision == "Approve");
        var rejected = logs.Count(l => l.Decision == "Reject");
        var reviewed = logs.Count(l => l.Decision == "Review");

        return Ok(new AnalyticsSummary(
            total,
            Math.Round(avgScore, 2),
            approved,
            reviewed,
            rejected,
            total > 0 ? Math.Round((double)approved / total * 100, 1) : 0
        ));
    }

    // ── Analysis Logs ─────────────────────────────────────────────────────
    [HttpGet("logs")]
    public async Task<ActionResult<List<AnalysisLog>>> GetLogs()
    {
        var logs = await _db.AnalysisLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(logs);
    }

    // ── Learn Content ─────────────────────────────────────────────────────
    [HttpGet("learn")]
    public async Task<ActionResult<List<LearnContent>>> GetLearnContent()
    {
        var items = await _db.LearnContents
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
        return Ok(items);
    }

    [HttpPut("learn/{id}")]
    public async Task<IActionResult> UpdateLearnContent(
        Guid id, [FromBody] LearnContent updated)
    {
        var item = await _db.LearnContents.FindAsync(id);
        if (item == null) return NotFound();

        item.ModelName = updated.ModelName;
        item.Summary = updated.Summary;
        item.Formula = updated.Formula;
        item.Explanation = updated.Explanation;
        item.RealExample = updated.RealExample;
        item.UsedIn = updated.UsedIn;
        item.IsActive = updated.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(item);
    }

    // ── Market Data ───────────────────────────────────────────────────────
    
    // NEW: Endpoint to actually fetch the market data!
    [HttpGet("market")]
    public async Task<ActionResult<MarketDataSnapshot>> GetMarketData()
    {
        var data = await _market.GetMarketDataAsync();
        return Ok(data);
    }

    [HttpPost("market/update")]
    public async Task<IActionResult> UpdateMarket(
        [FromBody] ManualMarketUpdate request)
    {
        _market.UpdateManualValues(
            request.RepoRate,
            request.InflationRate,
            request.GoldPer10g);
        return Ok(new { message = "Market data updated successfully." });
    }

    [HttpPost("market/refresh")]
    public async Task<IActionResult> RefreshMarket()
    {
        _market.InvalidateCache();
        var data = await _market.GetMarketDataAsync();
        return Ok(data);
    }
}

// ── Request / Response models ─────────────────────────────────────────────
public record LoginRequest(string Password);
public record LoginResponse(bool Success, string Token);
public record ManualMarketUpdate(
    double? RepoRate,
    double? InflationRate,
    double? GoldPer10g);
public record AnalyticsSummary(
    int TotalAnalyses,
    double AvgScore,
    int Approved,
    int Reviewed,
    int Rejected,
    double ApprovalRate);