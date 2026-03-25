using System.Text.Json;

namespace CreditRisk.API.Services;

public class MarketDataSnapshot
{
    public double RepoRate { get; set; } = 6.50;
    public double InflationRate { get; set; } = 4.85;
    public double Nifty50 { get; set; } = 22500;
    public double SensexValue { get; set; } = 74000;
    public double UsdInr { get; set; } = 83.50;
    public double GoldPer10g { get; set; } = 72000;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string DataSource { get; set; } = "Cached";
}

public class MarketDataService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<MarketDataService> _logger;

    // In-memory cache — refreshes every 24 hours automatically
    private MarketDataSnapshot _cache = new();
    private DateTime _lastFetch = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

    // Manual override values — set from admin panel
    // These persist in memory until the app restarts
    private double? _manualRepoRate = null;
    private double? _manualInflationRate = null;
    private double? _manualGoldPer10g = null;

    public MarketDataService(
        IHttpClientFactory httpFactory,
        ILogger<MarketDataService> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    // ── Main method called by controller ─────────────────────────────────
    public async Task<MarketDataSnapshot> GetMarketDataAsync()
    {
        if (DateTime.UtcNow - _lastFetch < _cacheDuration)
            return ApplyManualOverrides(_cache);

        try
        {
            var fresh = await FetchLiveDataAsync();
            _cache = fresh;
            _lastFetch = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Market data fetch failed: {msg}. Using cached data.", ex.Message);
            _cache.DataSource = "Cached (live fetch failed)";
        }

        return ApplyManualOverrides(_cache);
    }

    // ── Admin panel calls this to update manually maintained values ───────
    public void UpdateManualValues(
        double? repoRate, double? inflationRate, double? goldPer10g)
    {
        if (repoRate.HasValue) _manualRepoRate = repoRate;
        if (inflationRate.HasValue) _manualInflationRate = inflationRate;
        if (goldPer10g.HasValue) _manualGoldPer10g = goldPer10g;

        // Update cache immediately so next read reflects the new values
        _cache.LastUpdated = DateTime.UtcNow;
        _cache.DataSource = "Live + Admin Override";

        _logger.LogInformation(
            "Manual market values updated — Repo: {r}, Inflation: {i}, Gold: {g}",
            repoRate, inflationRate, goldPer10g);
    }

    // ── Force refresh — called from admin panel ───────────────────────────
    public void InvalidateCache()
    {
        _lastFetch = DateTime.MinValue;
    }

    // ── Apply admin overrides on top of live/cached data ─────────────────
    private MarketDataSnapshot ApplyManualOverrides(MarketDataSnapshot snapshot)
    {
        // Return a copy so we don't mutate the cached object
        return new MarketDataSnapshot
        {
            RepoRate = _manualRepoRate ?? snapshot.RepoRate,
            InflationRate = _manualInflationRate ?? snapshot.InflationRate,
            GoldPer10g = _manualGoldPer10g ?? snapshot.GoldPer10g,
            Nifty50 = snapshot.Nifty50,
            SensexValue = snapshot.SensexValue,
            UsdInr = snapshot.UsdInr,
            LastUpdated = snapshot.LastUpdated,
            DataSource = snapshot.DataSource
        };
    }

    // ── Fetch live data from free APIs ────────────────────────────────────
    private async Task<MarketDataSnapshot> FetchLiveDataAsync()
    {
        var snapshot = new MarketDataSnapshot();
        var client = _httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        // USD/INR — Frankfurter API (free, no key needed)
        try
        {
            var response = await client.GetStringAsync(
                "https://api.frankfurter.app/latest?from=USD&to=INR");
            var doc = JsonDocument.Parse(response);
            snapshot.UsdInr = Math.Round(
                doc.RootElement
                   .GetProperty("rates")
                   .GetProperty("INR")
                   .GetDouble(), 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("USD/INR fetch failed: {msg}", ex.Message);
        }

        // Nifty 50 — Yahoo Finance unofficial endpoint (free, no key)
        try
        {
            var response = await client.GetStringAsync(
                "https://query1.finance.yahoo.com/v8/finance/chart/%5ENSEI" +
                "?interval=1d&range=1d");
            var doc = JsonDocument.Parse(response);
            var price = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta")
                .GetProperty("regularMarketPrice")
                .GetDouble();
            snapshot.Nifty50 = Math.Round(price, 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Nifty 50 fetch failed: {msg}", ex.Message);
        }

        // Sensex — Yahoo Finance unofficial endpoint
        try
        {
            var response = await client.GetStringAsync(
                "https://query1.finance.yahoo.com/v8/finance/chart/%5EBSESN" +
                "?interval=1d&range=1d");
            var doc = JsonDocument.Parse(response);
            var price = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta")
                .GetProperty("regularMarketPrice")
                .GetDouble();
            snapshot.SensexValue = Math.Round(price, 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Sensex fetch failed: {msg}", ex.Message);
        }

        // Repo rate, inflation, gold — use manual overrides if set,
        // otherwise use last known good defaults
        snapshot.RepoRate = _manualRepoRate ?? 6.50;
        snapshot.InflationRate = _manualInflationRate ?? 4.85;
        snapshot.GoldPer10g = _manualGoldPer10g ?? 72000;
        snapshot.DataSource = "Live";
        snapshot.LastUpdated = DateTime.UtcNow;

        return snapshot;
    }
}