using System.Runtime.CompilerServices;

namespace CreditRisk.API.Models
{
    public class Applicant
    {
        public Guid ApplicantId { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; }=string.Empty;
        public decimal AnnualIncome { get; set; }
        public int CreditScore { get; set; }    //300-500
        public decimal DebtToIncomeRatio { get; set; }   //0.35=35%
        public double EmployementYears { get; set; }
        public DateTime CreteadAt { get; set; } = DateTime.UtcNow;

        //navigation property - one applicant can have many loan applications
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
}