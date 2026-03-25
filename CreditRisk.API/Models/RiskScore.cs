namespace CreditRisk.API.Models;

public class RiskScore
{
    public Guid ScoreId { get; set; } = Guid.NewGuid();

    // Foreign key — every score belongs to a loan application
    public Guid LoanId { get; set; }
    public LoanApplication LoanApplication { get; set; } = null!;

    public decimal Score { get; set; }          // 0.00 to 100.00
    public string Decision { get; set; } = string.Empty; // Approve/Review/Reject
    public string FactorsJson { get; set; } = "{}"; // JSON breakdown of why
    public string ModelVersion { get; set; } = "v1.0";
    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
}