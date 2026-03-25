import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { FinanceService } from '../../services/finance';
import { ScrollRevealDirective } from '../../directives/index';

@Component({
  selector:    'app-home',
  standalone:  true,
  imports:     [RouterLink, CommonModule, DecimalPipe, DatePipe, ScrollRevealDirective],
  templateUrl: './home.html',
  styleUrl:    './home.scss'
})
export class HomeComponent implements OnInit {

  private financeService = inject(FinanceService);

  loansProcessed = signal(0);
  avgRiskScore   = signal(0);
  portfolioValue = signal(0);
  approvalRate   = signal(0);

  marketData     = signal<any>(null);
  marketLoading  = signal(true);
  marketCardOpen = signal(true);

  journeys = [
    {
      icon:   'A',
      color:  'teal',
      title:  'Analyse My Loan Profile',
      desc:   'Enter your financial details and get an institutional-grade risk assessment. See what a bank would likely decide — and exactly why.',
      cta:    'Start Analysis',
      route:  '/apply',
      models: ['Sharpe', 'CAPM', 'Kelly', 'VaR', 'Merton']
    },
    {
      icon:   'I',
      color:  'amber',
      title:  'Plan Your Investments',
      desc:   'Calculate SIP corpus, track mutual fund NAV projections, and find your real CAGR on any investment over any time horizon.',
      cta:    'Open Calculator',
      route:  '/invest',
      models: ['SIP', 'CAGR', 'NAV', 'GBM']
    },
    {
      icon:   'C',
      color:  'purple',
      title:  'Loan vs Fund — Which Wins?',
      desc:   'Given your capital, should you take a loan or invest instead? Get a Sharpe-based comparison with a clear recommendation.',
      cta:    'Compare Now',
      route:  '/compare',
      models: ['Sharpe', 'Black-Scholes', 'Kelly']
    }
  ];

  models = [
    { name: 'Black-Scholes',   tag: 'Option Pricing'  },
    { name: 'CAPM',            tag: 'Risk-Return'      },
    { name: 'Sharpe Ratio',    tag: 'Risk-Adjusted'    },
    { name: 'Kelly Criterion', tag: 'Optimal Sizing'   },
    { name: 'Mean-Variance',   tag: 'Portfolio Opt.'   },
    { name: 'GBM',             tag: 'Price Process'    },
    { name: 'Value at Risk',   tag: 'Loss Metric'      },
    { name: 'Merton Model',    tag: 'Default Distance' },
    { name: 'SIP Calculator',  tag: 'Investment'       },
    { name: 'CAGR',            tag: 'Growth Rate'      },
    { name: 'NAV Tracker',     tag: 'Fund Analysis'    },
    { name: 'SIP vs Loan',     tag: 'Comparison'       },
  ];

  ngOnInit() {
    this.animateCounters();
    this.loadMarketData();
  }

  toggleMarketCard() {
    this.marketCardOpen.update(v => !v);
  }

  loadMarketData() {
    this.marketLoading.set(true);
    this.financeService.getMarketSnapshot().subscribe({
      next: (data) => {
        this.marketData.set(data);
        this.marketLoading.set(false);
      },
      error: () => {
        this.marketData.set({
          repoRate:      6.50,
          inflationRate: 4.85,
          nifty50:       22500,
          sensexValue:   74000,
          usdInr:        85.30,
          goldPer10g:    87000,
          dataSource:    'Fallback',
          lastUpdated:   new Date()
        });
        this.marketLoading.set(false);
      }
    });
  }

  animateCounters() {
    this.animateTo(v => this.loansProcessed.set(v), 0, 1284, 1800);
    this.animateTo(v => this.avgRiskScore.set(v),   0, 74,   1400);
    this.animateTo(v => this.portfolioValue.set(v), 0, 48,   2000);
    this.animateTo(v => this.approvalRate.set(v),   0, 68,   1600);
  }

  animateTo(setter: (v: number) => void, from: number, to: number, duration: number) {
    const start   = performance.now();
    const animate = (now: number) => {
      const progress = Math.min((now - start) / duration, 1);
      const eased    = 1 - Math.pow(1 - progress, 3);
      setter(Math.round(from + (to - from) * eased));
      if (progress < 1) requestAnimationFrame(animate);
    };
    requestAnimationFrame(animate);
  }
}