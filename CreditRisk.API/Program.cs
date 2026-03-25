using CreditRisk.API.Data;
using CreditRisk.API.FinanceEngine;
using CreditRisk.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Finance Engine ────────────────────────────────────────────────────────
builder.Services.AddScoped<FinanceEngineService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MarketDataService>();

// ── CORS — allows Angular (running on port 4200) to call this API ─────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200",
                            "https://localhost:4200")
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

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();