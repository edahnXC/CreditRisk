using Microsoft.EntityFrameworkCore;
using CreditRisk.API.Models;

namespace CreditRisk.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<RiskScore> RiskScores => Set<RiskScore>();
    public DbSet<LearnContent> LearnContents => Set<LearnContent>();
    public DbSet<AnalysisLog> AnalysisLogs => Set<AnalysisLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Applicant>(e =>
        {
            e.HasKey(a => a.ApplicantId);
            e.Property(a => a.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(a => a.Email).IsUnique();
            e.Property(a => a.FullName).IsRequired().HasMaxLength(200);
            e.Property(a => a.AnnualIncome).HasPrecision(18, 2);
            e.Property(a => a.DebtToIncomeRatio).HasPrecision(5, 4);
        });

        modelBuilder.Entity<LoanApplication>(e =>
        {
            e.HasKey(l => l.LoanId);
            e.Property(l => l.LoanAmount).HasPrecision(18, 2);
            e.Property(l => l.InterestRate).HasPrecision(5, 4);
            e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(l => l.Applicant)
             .WithMany(a => a.LoanApplications)
             .HasForeignKey(l => l.ApplicantId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RiskScore>(e =>
        {
            e.HasKey(r => r.ScoreId);
            e.Property(r => r.Score).HasPrecision(5, 2);
            e.HasOne(r => r.LoanApplication)
             .WithMany(l => l.RiskScores)
             .HasForeignKey(r => r.LoanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearnContent>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.ModelName).IsRequired().HasMaxLength(100);
            e.Property(l => l.Summary).HasMaxLength(500);
            e.Property(l => l.Formula).HasMaxLength(500);
        });

        modelBuilder.Entity<AnalysisLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.AnalysisType).HasMaxLength(50);
            e.Property(a => a.Decision).HasMaxLength(20);
        });

        SeedLearnContent(modelBuilder);
    }

    private static void SeedLearnContent(ModelBuilder modelBuilder)
    {
        var items = new List<LearnContent>
        {
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                ModelName   = "Sharpe Ratio",
                Abbr        = "SR",
                Category    = "Risk-Adjusted Return",
                Summary     = "Measures how much extra return you earn per unit of risk taken.",
                Formula     = "Sharpe = (Portfolio Return - Risk-Free Rate) / Std Dev of Returns",
                Explanation = "If two investments have the same return, the one with lower volatility has a higher Sharpe Ratio. A Sharpe above 1.0 is generally considered good. Above 2.0 is exceptional.",
                RealExample = "A loan at 9% interest with 12% return volatility and a 4% risk-free rate has a Sharpe of (0.09 - 0.04) / 0.12 = 0.42. That is poor.",
                UsedIn      = "Loan Analysis, Portfolio Comparison",
                SortOrder   = 1, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                ModelName   = "CAPM",
                Abbr        = "C",
                Category    = "Risk-Return Model",
                Summary     = "Calculates the minimum return an investment must earn given its market risk.",
                Formula     = "Expected Return = Risk-Free Rate + Beta x (Market Return - Risk-Free Rate)",
                Explanation = "Beta measures how sensitive an asset is to market movements. A salaried employee has low beta. A business owner has high beta. CAPM tells you what return you should demand.",
                RealExample = "A self-employed borrower with Beta 1.2, market return 12%, risk-free rate 6.5% needs a return of 6.5% + 1.2 x (12% - 6.5%) = 13.1%.",
                UsedIn      = "Loan Analysis",
                SortOrder   = 2, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000003"),
                ModelName   = "Kelly Criterion",
                Abbr        = "KC",
                Category    = "Optimal Bet Sizing",
                Summary     = "Tells you what fraction of your capital to allocate to maximise long-term growth.",
                Formula     = "Kelly = (Win Probability x Win Ratio - Loss Probability) / Win Ratio",
                Explanation = "In lending, winning means the loan gets repaid with interest. Kelly tells a lender how much of their total capital should go into any single loan. Professional investors almost always use Half-Kelly for safety.",
                RealExample = "If repayment probability is 92% and interest earned is 9% of principal, Kelly fraction is approximately 3% of capital per loan.",
                UsedIn      = "Loan Analysis, Portfolio Comparison",
                SortOrder   = 3, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000004"),
                ModelName   = "Value at Risk",
                Abbr        = "VaR",
                Category    = "Portfolio Loss Metric",
                Summary     = "The maximum loss you can expect with a given confidence level over a time period.",
                Formula     = "VaR = Loan Amount x Volatility x Z-score x sqrt(Days / 252)",
                Explanation = "VaR answers: what is the worst-case loss we expect 95% of the time over the next 30 days? Banks use VaR daily to manage risk exposure across entire portfolios.",
                RealExample = "A 5,00,000 loan with 15% annual volatility has a 30-day 95% VaR of approximately 35,826. You should not lose more than this in any 30-day period with 95% confidence.",
                UsedIn      = "Loan Analysis",
                SortOrder   = 4, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000005"),
                ModelName   = "Geometric Brownian Motion",
                Abbr        = "GBM",
                Category    = "Price Process Model",
                Summary     = "Models how income or asset prices evolve randomly over time.",
                Formula     = "S(t+1) = S(t) x exp((mu - sigma^2/2) x dt + sigma x sqrt(dt) x Z)",
                Explanation = "GBM is the same mathematics used to model stock prices. Applied to lending it simulates 1000 possible futures for an applicant income. If many paths fall below the minimum repayment threshold the loan is high stress-risk.",
                RealExample = "A borrower earning 8L per year with 15% income volatility — GBM simulates their income over 5 years. If 18% of paths drop below 5L minimum needed to repay, stress probability is 18%.",
                UsedIn      = "NAV Simulator",
                SortOrder   = 5, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000006"),
                ModelName   = "Black-Scholes",
                Abbr        = "BS",
                Category    = "Option Pricing Model",
                Summary     = "Prices the value of a financial option — the right but not obligation to do something.",
                Formula     = "Call = S x N(d1) - K x e^(-rT) x N(d2)",
                Explanation = "In lending a borrower right to prepay is a financial option. When interest rates fall borrowers refinance. Black-Scholes quantifies exactly how much that prepayment right is worth — a cost to the lender in lost future interest.",
                RealExample = "A 5,00,000 loan at 9% for 5 years has a prepayment option worth approximately 28,000 if volatility is 20%. The lender should demand slightly higher interest to compensate.",
                UsedIn      = "Loan Analysis, Portfolio Comparison",
                SortOrder   = 6, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000007"),
                ModelName   = "Mean-Variance Optimization",
                Abbr        = "MV",
                Category    = "Portfolio Optimization",
                Summary     = "Finds the ideal mix of assets that maximises return for a given level of risk.",
                Formula     = "Minimise w'Sigma*w subject to w'mu = target return",
                Explanation = "Introduced by Harry Markowitz in 1952. Shows that diversification mathematically reduces risk without sacrificing return. The Efficient Frontier is the curve of all optimal portfolios.",
                RealExample = "A portfolio of 60% home loans and 40% personal loans might have lower combined volatility than either loan type alone because their defaults do not perfectly correlate.",
                UsedIn      = "Admin Portfolio Dashboard",
                SortOrder   = 7, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000008"),
                ModelName   = "Merton Model",
                Abbr        = "M",
                Category    = "Default Distance",
                Summary     = "Treats a borrower assets as a stock — estimates how far they are from defaulting.",
                Formula     = "Distance to Default = (ln(A/D) + (r - sigma^2/2) x T) / (sigma x sqrt(T))",
                Explanation = "Robert Merton showed that equity is mathematically a call option on assets. If assets fall below total debt default occurs. Distance to Default measures this in standard deviations — higher is safer.",
                RealExample = "A borrower with 12L in assets and 5L in debt with 15% asset volatility has a Distance to Default of 2.8 standard deviations. Default probability is approximately 2.5%.",
                UsedIn      = "Loan Analysis",
                SortOrder   = 8, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000009"),
                ModelName   = "SIP Calculator",
                Abbr        = "SIP",
                Category    = "Investment Planning",
                Summary     = "Calculates how a fixed monthly investment grows into a corpus over time.",
                Formula     = "FV = P x [((1 + r)^n - 1) / r] x (1 + r)",
                Explanation = "SIP is the most common way Indians invest in mutual funds. By investing a fixed amount every month you buy more units when prices are low. The power of compounding means small regular investments grow into large corpuses over time.",
                RealExample = "Investing 5,000 per month for 10 years at 12% annual return gives a corpus of 11.6L on an investment of just 6L — a wealth gain of 5.6L.",
                UsedIn      = "Investment Hub",
                SortOrder   = 9, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000010"),
                ModelName   = "CAGR",
                Abbr        = "CAGR",
                Category    = "Growth Rate",
                Summary     = "The true annualised growth rate of an investment accounting for compounding.",
                Formula     = "CAGR = (Final Value / Initial Value) ^ (1 / Years) - 1",
                Explanation = "Absolute return tells you the total gain. CAGR tells you the annual rate at which that gain compounded. All mutual fund fact sheets in India are required to report CAGR.",
                RealExample = "1,00,000 growing to 2,50,000 in 7 years has an absolute return of 150% but a CAGR of 13.98% per year.",
                UsedIn      = "Investment Hub",
                SortOrder   = 10, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000011"),
                ModelName   = "NAV Tracker",
                Abbr        = "NAV",
                Category    = "Fund Analysis",
                Summary     = "Simulates how a mutual fund NAV might evolve using Monte Carlo methods.",
                Formula     = "Uses Geometric Brownian Motion with fund-specific drift and volatility",
                Explanation = "NAV is the price of one unit of a mutual fund. Our simulator runs 500 GBM paths to show the range of possible outcomes — bull case, expected median, and bear case.",
                RealExample = "A fund starting at NAV 10 with 12% expected return and 18% volatility over 5 years shows Bull case 28, Expected 18, Bear case 11.",
                UsedIn      = "Investment Hub",
                SortOrder   = 11, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            },
            new() {
                Id          = Guid.Parse("11111111-0000-0000-0000-000000000012"),
                ModelName   = "SIP vs Loan",
                Abbr        = "SVL",
                Category    = "Comparison Tool",
                Summary     = "Compares deploying capital as a loan versus investing it in a mutual fund using Sharpe Ratios.",
                Formula     = "Loan Effective Return = Interest Rate x (1 - Default Probability)",
                Explanation = "The core question for any capital allocator: given a sum of money should I lend it or invest it? This tool answers using Sharpe Ratios — whichever gives more return per unit of risk is the better choice.",
                RealExample = "5,00,000 as a 9% loan with 8% default risk has Sharpe 0.55. Same amount in a fund at 12% return and 18% volatility has Sharpe 0.44. The loan wins on risk-adjusted terms.",
                UsedIn      = "Compare Tool",
                SortOrder   = 12, IsActive = true, UpdatedAt = new DateTime(2024,1,1)
            }
        };

        modelBuilder.Entity<LearnContent>().HasData(items);
    }
}