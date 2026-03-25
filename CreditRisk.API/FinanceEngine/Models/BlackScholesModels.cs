namespace CreditRisk.API.FinanceEngine.Models;

public record BlackScholesInput(
    double S,
    double K,
    double T,
    double R,
    double Sigma
);

public record BlackScholesResult(
    double CallPrice,
    double PutPrice,
    double Delta,
    double Gamma,
    double Vega,
    double Theta,
    string Interpretation
);