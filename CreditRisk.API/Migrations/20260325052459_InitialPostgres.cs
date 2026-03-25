using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CreditRisk.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompositeScore = table.Column<double>(type: "double precision", nullable: false),
                    Decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LoanAmount = table.Column<double>(type: "double precision", nullable: false),
                    InterestRate = table.Column<double>(type: "double precision", nullable: false),
                    InputJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditScore = table.Column<int>(type: "integer", nullable: false),
                    DebtToIncomeRatio = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    EmployementYears = table.Column<double>(type: "double precision", nullable: false),
                    CreteadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                });

            migrationBuilder.CreateTable(
                name: "LearnContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Abbr = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Formula = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: false),
                    RealExample = table.Column<string>(type: "text", nullable: false),
                    UsedIn = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LoanPurpose = table.Column<string>(type: "text", nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    TermMonths = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.LoanId);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskScores",
                columns: table => new
                {
                    ScoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Decision = table.Column<string>(type: "text", nullable: false),
                    FactorsJson = table.Column<string>(type: "text", nullable: false),
                    ModelVersion = table.Column<string>(type: "text", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskScores", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_RiskScores_LoanApplications_LoanId",
                        column: x => x.LoanId,
                        principalTable: "LoanApplications",
                        principalColumn: "LoanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LearnContents",
                columns: new[] { "Id", "Abbr", "Category", "Explanation", "Formula", "IsActive", "ModelName", "RealExample", "SortOrder", "Summary", "UpdatedAt", "UsedIn" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "SR", "Risk-Adjusted Return", "If two investments have the same return, the one with lower volatility has a higher Sharpe Ratio. A Sharpe above 1.0 is generally considered good. Above 2.0 is exceptional.", "Sharpe = (Portfolio Return - Risk-Free Rate) / Std Dev of Returns", true, "Sharpe Ratio", "A loan at 9% interest with 12% return volatility and a 4% risk-free rate has a Sharpe of (0.09 - 0.04) / 0.12 = 0.42. That is poor.", 1, "Measures how much extra return you earn per unit of risk taken.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Loan Analysis, Portfolio Comparison" },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "C", "Risk-Return Model", "Beta measures how sensitive an asset is to market movements. A salaried employee has low beta. A business owner has high beta. CAPM tells you what return you should demand.", "Expected Return = Risk-Free Rate + Beta x (Market Return - Risk-Free Rate)", true, "CAPM", "A self-employed borrower with Beta 1.2, market return 12%, risk-free rate 6.5% needs a return of 6.5% + 1.2 x (12% - 6.5%) = 13.1%.", 2, "Calculates the minimum return an investment must earn given its market risk.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Loan Analysis" },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "KC", "Optimal Bet Sizing", "In lending, winning means the loan gets repaid with interest. Kelly tells a lender how much of their total capital should go into any single loan. Professional investors almost always use Half-Kelly for safety.", "Kelly = (Win Probability x Win Ratio - Loss Probability) / Win Ratio", true, "Kelly Criterion", "If repayment probability is 92% and interest earned is 9% of principal, Kelly fraction is approximately 3% of capital per loan.", 3, "Tells you what fraction of your capital to allocate to maximise long-term growth.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Loan Analysis, Portfolio Comparison" },
                    { new Guid("11111111-0000-0000-0000-000000000004"), "VaR", "Portfolio Loss Metric", "VaR answers: what is the worst-case loss we expect 95% of the time over the next 30 days? Banks use VaR daily to manage risk exposure across entire portfolios.", "VaR = Loan Amount x Volatility x Z-score x sqrt(Days / 252)", true, "Value at Risk", "A 5,00,000 loan with 15% annual volatility has a 30-day 95% VaR of approximately 35,826. You should not lose more than this in any 30-day period with 95% confidence.", 4, "The maximum loss you can expect with a given confidence level over a time period.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Loan Analysis" },
                    { new Guid("11111111-0000-0000-0000-000000000005"), "GBM", "Price Process Model", "GBM is the same mathematics used to model stock prices. Applied to lending it simulates 1000 possible futures for an applicant income. If many paths fall below the minimum repayment threshold the loan is high stress-risk.", "S(t+1) = S(t) x exp((mu - sigma^2/2) x dt + sigma x sqrt(dt) x Z)", true, "Geometric Brownian Motion", "A borrower earning 8L per year with 15% income volatility — GBM simulates their income over 5 years. If 18% of paths drop below 5L minimum needed to repay, stress probability is 18%.", 5, "Models how income or asset prices evolve randomly over time.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NAV Simulator" },
                    { new Guid("11111111-0000-0000-0000-000000000006"), "BS", "Option Pricing Model", "In lending a borrower right to prepay is a financial option. When interest rates fall borrowers refinance. Black-Scholes quantifies exactly how much that prepayment right is worth — a cost to the lender in lost future interest.", "Call = S x N(d1) - K x e^(-rT) x N(d2)", true, "Black-Scholes", "A 5,00,000 loan at 9% for 5 years has a prepayment option worth approximately 28,000 if volatility is 20%. The lender should demand slightly higher interest to compensate.", 6, "Prices the value of a financial option — the right but not obligation to do something.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Loan Analysis, Portfolio Comparison" },
                    { new Guid("11111111-0000-0000-0000-000000000007"), "MV", "Portfolio Optimization", "Introduced by Harry Markowitz in 1952. Shows that diversification mathematically reduces risk without sacrificing return. The Efficient Frontier is the curve of all optimal portfolios.", "Minimise w'Sigma*w subject to w'mu = target return", true, "Mean-Variance Optimization", "A portfolio of 60% home loans and 40% personal loans might have lower combined volatility than either loan type alone because their defaults do not perfectly correlate.", 7, "Finds the ideal mix of assets that maximises return for a given level of risk.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin Portfolio Dashboard" },
                    { new Guid("11111111-0000-0000-0000-000000000008"), "M", "Default Distance", "Robert Merton showed that equity is mathematically a call option on assets. If assets fall below total debt default occurs. Distance to Default measures this in standard deviations — higher is safer.", "Distance to Default = (ln(A/D) + (r - sigma^2/2) x T) / (sigma x sqrt(T))", true, "Merton Model", "A borrower with 12L in assets and 5L in debt with 15% asset volatility has a Distance to Default of 2.8 standard deviations. Default probability is approximately 2.5%.", 8, "Treats a borrower assets as a stock — estimates how far they are from defaulting.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Loan Analysis" },
                    { new Guid("11111111-0000-0000-0000-000000000009"), "SIP", "Investment Planning", "SIP is the most common way Indians invest in mutual funds. By investing a fixed amount every month you buy more units when prices are low. The power of compounding means small regular investments grow into large corpuses over time.", "FV = P x [((1 + r)^n - 1) / r] x (1 + r)", true, "SIP Calculator", "Investing 5,000 per month for 10 years at 12% annual return gives a corpus of 11.6L on an investment of just 6L — a wealth gain of 5.6L.", 9, "Calculates how a fixed monthly investment grows into a corpus over time.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Investment Hub" },
                    { new Guid("11111111-0000-0000-0000-000000000010"), "CAGR", "Growth Rate", "Absolute return tells you the total gain. CAGR tells you the annual rate at which that gain compounded. All mutual fund fact sheets in India are required to report CAGR.", "CAGR = (Final Value / Initial Value) ^ (1 / Years) - 1", true, "CAGR", "1,00,000 growing to 2,50,000 in 7 years has an absolute return of 150% but a CAGR of 13.98% per year.", 10, "The true annualised growth rate of an investment accounting for compounding.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Investment Hub" },
                    { new Guid("11111111-0000-0000-0000-000000000011"), "NAV", "Fund Analysis", "NAV is the price of one unit of a mutual fund. Our simulator runs 500 GBM paths to show the range of possible outcomes — bull case, expected median, and bear case.", "Uses Geometric Brownian Motion with fund-specific drift and volatility", true, "NAV Tracker", "A fund starting at NAV 10 with 12% expected return and 18% volatility over 5 years shows Bull case 28, Expected 18, Bear case 11.", 11, "Simulates how a mutual fund NAV might evolve using Monte Carlo methods.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Investment Hub" },
                    { new Guid("11111111-0000-0000-0000-000000000012"), "SVL", "Comparison Tool", "The core question for any capital allocator: given a sum of money should I lend it or invest it? This tool answers using Sharpe Ratios — whichever gives more return per unit of risk is the better choice.", "Loan Effective Return = Interest Rate x (1 - Default Probability)", true, "SIP vs Loan", "5,00,000 as a 9% loan with 8% default risk has Sharpe 0.55. Same amount in a fund at 12% return and 18% volatility has Sharpe 0.44. The loan wins on risk-adjusted terms.", 12, "Compares deploying capital as a loan versus investing it in a mutual fund using Sharpe Ratios.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Compare Tool" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Email",
                table: "Applicants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_ApplicantId",
                table: "LoanApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskScores_LoanId",
                table: "RiskScores",
                column: "LoanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisLogs");

            migrationBuilder.DropTable(
                name: "LearnContents");

            migrationBuilder.DropTable(
                name: "RiskScores");

            migrationBuilder.DropTable(
                name: "LoanApplications");

            migrationBuilder.DropTable(
                name: "Applicants");
        }
    }
}
