import { Component, signal, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FinanceService } from '../../services/finance';
import { ScrollRevealDirective } from '../../directives/index';

@Component({
  selector:    'app-invest',
  standalone:  true,
  imports:     [CommonModule, FormsModule, RouterLink,
                DecimalPipe, ScrollRevealDirective],
  templateUrl: './invest.html',
  styleUrl:    './invest.scss'
})
export class InvestComponent {

  private financeService = inject(FinanceService);

  Math=Math;
  
  activeTab = signal<'sip' | 'cagr' | 'nav'>('sip');

  // ── SIP state ─────────────────────────────────────────────────────────
  sipForm = {
    monthlyInvestment: 5000,
    annualReturnRate:  12,
    investmentYears:   10
  };
  sipResult   = signal<any>(null);
  sipLoading  = signal(false);

  // ── CAGR state ────────────────────────────────────────────────────────
  cagrForm = {
    initialValue: 100000,
    finalValue:   250000,
    years:        7
  };
  cagrResult  = signal<any>(null);
  cagrLoading = signal(false);

  // ── NAV state ─────────────────────────────────────────────────────────
  navForm = {
    initialNAV:       10,
    annualReturnRate: 12,
    annualVolatility: 18,
    years:            5,
    numSimulations:   500
  };
  navResult  = signal<any>(null);
  navLoading = signal(false);

  setTab(tab: 'sip' | 'cagr' | 'nav') {
  this.activeTab.set(tab);
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

  // ── SIP calculation ───────────────────────────────────────────────────
  calculateSIP() {
    this.sipLoading.set(true);
    this.sipResult.set(null);

    this.financeService.calculateSIP({
      monthlyInvestment: this.sipForm.monthlyInvestment,
      annualReturnRate:  this.sipForm.annualReturnRate / 100,
      investmentMonths:  this.sipForm.investmentYears * 12
    }).subscribe({
      next:  (data) => { this.sipResult.set(data);  this.sipLoading.set(false); },
      error: ()     => { this.sipLoading.set(false); }
    });
  }

  // ── CAGR calculation ──────────────────────────────────────────────────
  calculateCAGR() {
    this.cagrLoading.set(true);
    this.cagrResult.set(null);

    this.financeService.calculateCAGR({
      initialValue: this.cagrForm.initialValue,
      finalValue:   this.cagrForm.finalValue,
      years:        this.cagrForm.years
    }).subscribe({
      next:  (data) => { this.cagrResult.set(data);  this.cagrLoading.set(false); },
      error: ()     => { this.cagrLoading.set(false); }
    });
  }

  // ── NAV simulation ────────────────────────────────────────────────────
  simulateNAV() {
    this.navLoading.set(true);
    this.navResult.set(null);

    this.financeService.trackNAV({
      initialNAV:        this.navForm.initialNAV,
      annualReturnRate:  this.navForm.annualReturnRate / 100,
      annualVolatility:  this.navForm.annualVolatility / 100,
      years:             this.navForm.years,
      numSimulations:    this.navForm.numSimulations
    }).subscribe({
      next:  (data) => { this.navResult.set(data);  this.navLoading.set(false); },
      error: ()     => { this.navLoading.set(false); }
    });
  }

  formatCurrency(value: number): string {
    if (value >= 10000000) return '₹' + (value / 10000000).toFixed(2) + ' Cr';
    if (value >= 100000)   return '₹' + (value / 100000).toFixed(2) + ' L';
    return '₹' + Math.round(value).toLocaleString('en-IN');
  }

  getCAGRColor(cagr: number): string {
    if (cagr > 0.12) return 'approve';
    if (cagr > 0.06) return 'review';
    return 'reject';
  }
}