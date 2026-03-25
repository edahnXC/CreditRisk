namespace CreditRisk.API.Models;

public class AnalysisLog
{
    public Guid     Id             { get; set; } = Guid.NewGuid();
    public string   AnalysisType   { get; set; } = string.Empty;
    public double   CompositeScore { get; set; }
    public string   Decision       { get; set; } = string.Empty;
    public double   LoanAmount     { get; set; }
    public double   InterestRate   { get; set; }
    public string   InputJson      { get; set; } = "{}";
    public string   ResultJson     { get; set; } = "{}";
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
}