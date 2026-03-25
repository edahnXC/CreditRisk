using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditRisk.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: false),
                    DebtToIncomeRatio = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    EmployementYears = table.Column<double>(type: "float", nullable: false),
                    CreteadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LoanPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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
                    ScoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "RiskScores");

            migrationBuilder.DropTable(
                name: "LoanApplications");

            migrationBuilder.DropTable(
                name: "Applicants");
        }
    }
}
