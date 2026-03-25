namespace CreditRisk.API.Models;

public class LearnContent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ModelName { get; set; } = string.Empty;
    public string Abbr { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string RealExample { get; set; } = string.Empty;
    public string UsedIn { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}