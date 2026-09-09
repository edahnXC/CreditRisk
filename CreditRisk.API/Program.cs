using System.Text;
using CreditRisk.API.Data;
using CreditRisk.API.FinanceEngine;
using CreditRisk.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

// ── Authentication (JWT Bearer) ───────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CreditRiskSystemSuperSecretKey2024!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CreditRisk.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CreditRisk.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtIssuer,
        ValidateAudience         = true,
        ValidAudience            = jwtAudience,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.FromMinutes(1)
    };
});

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

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- Auto-Migrate Database on Startup ---
// This will automatically create your tables in Neon when Render spins up!
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.Run();