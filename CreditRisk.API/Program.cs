using CreditRisk.API.Data;
using CreditRisk.API.FinanceEngine;
using CreditRisk.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
// Changed to UseNpgsql for your Neon PostgreSQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Finance Engine ────────────────────────────────────────────────────────
builder.Services.AddScoped<FinanceEngineService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MarketDataService>();

// ── CORS — allows Angular (Netlify & localhost) to call this API ──────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend", policy =>
        policy.WithOrigins("https://creditsrisksystem.netlify.app",
                            "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Credit Risk API",
        Version = "v1",
        Description = "AI-Powered Credit Risk Scoring & Loan Decision System"
    });
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Credit Risk API v1"));
}

// Fixed: The name here must exactly match the policy name defined above
app.UseCors("AllowMyFrontend"); 

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();