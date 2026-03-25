import { Component, signal, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FinanceService } from '../../services/finance';
import { ScrollRevealDirective } from '../../directives/index';

@Component({
  selector:    'app-apply',
  standalone:  true,
  imports:     [CommonModule, FormsModule, RouterLink,
                DecimalPipe, ScrollRevealDirective],
  templateUrl: './apply.html',
  styleUrl:    './apply.scss'
})
export class ApplyComponent {

  private financeService = inject(FinanceService);

  currentStep = signal(1);
  totalSteps  = 2;
  loading     = signal(false);
  result      = signal<any>(null);

  form = {
    // Step 1 — Financial details
    annualIncome:     800000,
    existingDebt:     200000,
    creditScore:      700,
    employmentType:   'Salaried',
    employmentYears:  5,

    // Step 2 — Loan details
    loanAmount:       500000,
    loanPurpose:      'Personal',
    loanTermYears:    5,
    interestRate:     9.5,
  };

  employmentTypes = ['Salaried', 'Self-Employed', 'Business Owner', 'Freelancer'];
  loanPurposes    = ['Personal', 'Home', 'Auto', 'Education', 'Business'];

  private betaMap: Record<string, number> = {
    'Salaried':       0.6,
    'Self-Employed':  1.1,
    'Business Owner': 1.3,
    'Freelancer':     1.2
  };

  get debtToIncomeRatio(): number {
    return this.form.annualIncome > 0
      ? Math.min(this.form.existingDebt / this.form.annualIncome, 1)
      : 0;
  }

  get defaultProbability(): number {
    const creditFactor = (this.form.creditScore - 300) / 550;
    const dtiFactor    = Math.min(this.debtToIncomeRatio, 1);
    return Math.max(0.01, Math.min(0.6,
      0.5 - creditFactor * 0.4 - (1 - dtiFactor) * 0.1));
  }

  get monthlyEMI(): number {
    const r = (this.form.interestRate / 100) / 12;
    const n = this.form.loanTermYears * 12;
    if (r === 0) return this.form.loanAmount / n;
    return this.form.loanAmount * r * Math.pow(1 + r, n)
           / (Math.pow(1 + r, n) - 1);
  }

  get totalPayable(): number {
    return this.monthlyEMI * this.form.loanTermYears * 12;
  }

  nextStep() {
    if (this.currentStep() < this.totalSteps)
      this.currentStep.update(v => v + 1);
  }

  prevStep() {
    if (this.currentStep() > 1)
      this.currentStep.update(v => v - 1);
  }

  canProceed(): boolean {
    switch (this.currentStep()) {
      case 1:  return this.form.annualIncome > 0 && this.form.creditScore >= 300;
      case 2:  return this.form.loanAmount > 0;
      default: return true;
    }
  }

  analyse() {
    this.loading.set(true);
    this.result.set(null);

    const rate      = this.form.interestRate / 100;
    const beta      = this.betaMap[this.form.employmentType] ?? 1.0;
    const assets    = this.form.annualIncome * 5;
    const returnVol = 0.08 + this.defaultProbability * 0.15;

    const payload = {
      loanAmount:         this.form.loanAmount,
      interestRate:       rate,
      riskFreeRate:       0.065,
      marketReturn:       0.12,
      beta:               beta,
      returnVolatility:   returnVol,
      defaultProbability: this.defaultProbability,
      loanTermYears:      this.form.loanTermYears,
      applicantAssets:    assets,
      assetVolatility:    0.15 + this.defaultProbability * 0.1
    };

    this.financeService.getLoanRiskProfile(payload).subscribe({
      next: (data) => {
        this.result.set(data);
        this.loading.set(false);
        this.currentStep.set(3);
      },
      error: () => { this.loading.set(false); }
    });
  }

  resetForm() {
    this.currentStep.set(1);
    this.result.set(null);
  }

  getBankLikelihood(score: number): string {
    if (score >= 70) return 'High';
    if (score >= 40) return 'Moderate';
    return 'Low';
  }

  getBankLikelihoodDesc(score: number): string {
    if (score >= 70)
      return 'Based on your profile, most banks would likely approve this loan. Your risk indicators are within acceptable ranges.';
    if (score >= 40)
      return 'Your profile falls in a grey zone. Some banks may approve with conditions — higher interest rate or collateral may be required.';
    return 'This profile presents elevated default risk. Most banks would likely decline. Consider reducing the loan amount or improving your credit score first.';
  }

  getScoreColor(score: number): string {
    if (score >= 70) return 'approve';
    if (score >= 40) return 'review';
    return 'reject';
  }

  formatCurrency(value: number): string {
    if (value >= 10000000) return '₹' + (value / 10000000).toFixed(1) + 'Cr';
    if (value >= 100000)   return '₹' + (value / 100000).toFixed(1) + 'L';
    return '₹' + Math.round(value).toLocaleString('en-IN');
  }
}