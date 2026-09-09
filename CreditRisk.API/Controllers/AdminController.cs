using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    private readonly IConfiguration _config;

    public AdminController(AppDbContext db, MarketDataService market, IConfiguration config)
    {
        _db     = db;
        _market = market;
        _config = config;
    }

    // ── Auth ──────────────────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var expectedPassword = _config["ADMIN_PASSWORD"] 
                            ?? _config["AdminSettings:Password"] 
                            ?? "Admin@2025!";

        if (string.IsNullOrWhiteSpace(request?.Password) || request.Password != expectedPassword)
        {
            return Unauthorized(new LoginResponse(false, "", null, "Invalid administrator credentials."));
        }

        // Generate signed JWT Token
        var jwtKey = _config["Jwt:Key"] ?? "CreditRiskSystemSuperSecretKey2024!!";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "CreditRisk.API";
        var jwtAudience = _config["Jwt:Audience"] ?? "CreditRisk.Client";
        var expiryMins = double.TryParse(_config["Jwt:ExpiryMinutes"], out var mins) ? mins : 480;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMins);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "AdminUser"),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires            = expiresAt,
            Issuer             = jwtIssuer,
            Audience           = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new LoginResponse(true, tokenString, expiresAt, "Login successful."));
    }

    // ── Token Verification ────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    [HttpGet("verify")]
    public IActionResult VerifyToken()
    {
        return Ok(new { valid = true, user = User.Identity?.Name ?? "Admin" });
    }

    // ── Analytics (Public for dashboard display) ───────────────────────────
    [AllowAnonymous]
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

    // ── Analysis Logs (Admin Protected) ───────────────────────────────────
    [Authorize(Roles = "Admin")]
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
    // Public read for learning library
    [AllowAnonymous]
    [HttpGet("learn")]
    public async Task<ActionResult<List<LearnContent>>> GetLearnContent()
    {
        var items = await _db.LearnContents
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
        return Ok(items);
    }

    // Admin protected for content updates
    [Authorize(Roles = "Admin")]
    [HttpPut("learn/{id}")]
    public async Task<IActionResult> UpdateLearnContent(
        Guid id, [FromBody] LearnContent updated)
    {
        var item = await _db.LearnContents.FindAsync(id);
        if (item == null) return NotFound();

        item.ModelName   = updated.ModelName;
        item.Summary     = updated.Summary;
        item.Formula     = updated.Formula;
        item.Explanation = updated.Explanation;
        item.RealExample = updated.RealExample;
        item.UsedIn      = updated.UsedIn;
        item.IsActive    = updated.IsActive;
        item.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(item);
    }

    // ── Market Data ───────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    [HttpGet("market")]
    public async Task<ActionResult<MarketDataSnapshot>> GetMarketData()
    {
        var data = await _market.GetMarketDataAsync();
        return Ok(data);
    }

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
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
public record LoginResponse(bool Success, string Token, DateTime? ExpiresAt = null, string? Message = null);
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