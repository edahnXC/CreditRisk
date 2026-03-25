import { Component, signal, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FinanceService } from '../../services/finance';
import { ScrollRevealDirective } from '../../directives/index';

@Component({
  selector:    'app-compare',
  standalone:  true,
  imports:     [CommonModule, FormsModule, RouterLink,
                DecimalPipe, ScrollRevealDirective],
  templateUrl: './compare.html',
  styleUrl:    './compare.scss'
})
export class CompareComponent {

  private financeService = inject(FinanceService);

  Math=Math;

  loading = signal(false);
  result  = signal<any>(null);

  form = {
    capital:               500000,
    years:                 10,
    riskFreeRate:          6.5,

    // Loan side
    loanInterestRate:      9.5,
    loanDefaultProb:       8,
    loanVolatility:        14,

    // Fund side
    fundAnnualReturn:      12,
    fundVolatility:        18,
  };

  compare() {
    this.loading.set(true);
    this.result.set(null);

    this.financeService.compareSipVsLoan({
      capital:               this.form.capital,
      years:                 this.form.years,
      riskFreeRate:          this.form.riskFreeRate / 100,
      loanInterestRate:      this.form.loanInterestRate / 100,
      loanDefaultProbability: this.form.loanDefaultProb / 100,
      loanVolatility:        this.form.loanVolatility / 100,
      fundAnnualReturn:      this.form.fundAnnualReturn / 100,
      fundVolatility:        this.form.fundVolatility / 100,
    }).subscribe({
      next:  (data) => { this.result.set(data);  this.loading.set(false); },
      error: ()     => { this.loading.set(false); }
    });
  }

  reset() {
    this.result.set(null);
  }

  formatCurrency(value: number): string {
    if (value >= 10000000) return '₹' + (value / 10000000).toFixed(2) + ' Cr';
    if (value >= 100000)   return '₹' + (value / 100000).toFixed(2) + ' L';
    return '₹' + Math.round(value).toLocaleString('en-IN');
  }

  getWinner(): 'loan' | 'fund' | 'tie' {
    if (!this.result()) return 'tie';
    const adv = this.result().sharpeAdvantage;
    if (adv >  0.2) return 'fund';
    if (adv < -0.2) return 'loan';
    return 'tie';
  }

  getSharpeColor(sharpe: number): string {
    if (sharpe > 1.0) return 'approve';
    if (sharpe > 0.5) return 'review';
    return 'reject';
  }
}