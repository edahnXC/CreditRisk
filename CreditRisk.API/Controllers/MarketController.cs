using Microsoft.AspNetCore.Mvc;
using CreditRisk.API.Services;

namespace CreditRisk.API.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly MarketDataService _market;

    public MarketController(MarketDataService market)
    {
        _market = market;
    }

    // This perfectly matches the exact URL your Angular app is calling!
    [HttpGet("snapshot")]
    public async Task<ActionResult<MarketDataSnapshot>> GetSnapshot()
    {
        var data = await _market.GetMarketDataAsync();
        return Ok(data);
    }
}