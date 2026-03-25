namespace CreditRisk.API.Models
{
    public class LoanApplication
    {
        public Guid LoanId { get; set; } = Guid.NewGuid();
        
        //foreign key - links back to the applicant
        public Guid ApplicantId { get; set; }
        public Applicant Applicant { get; set; } = null!; //navigation property
        public decimal LoanAmount { get; set; }
        public string LoanPurpose { get; set; } = string.Empty; //home, auto, purpose
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public string Status { get; set; } = "Pending"; //Pending, Approved, Rejected   

        //naviagtion property - one loan can have many rishk score records
        public ICollection<RiskScore> RiskScores { get; set; } = new List<RiskScore>();
    }
}
