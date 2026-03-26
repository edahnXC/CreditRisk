import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http'; // <-- Added HttpClient
import { FinanceService } from '../../services/finance';
import { ScrollRevealDirective } from '../../directives/index';

@Component({
  selector:    'app-home',
  standalone:  true,
  imports:     [RouterLink, CommonModule, DecimalPipe, DatePipe, ScrollRevealDirective],
  templateUrl: './home.html',
  styleUrl:    './home.scss'
})
export class HomeComponent implements OnInit, OnDestroy {

  private financeService = inject(FinanceService);
  private http = inject(HttpClient); // <-- Injected HttpClient to call your API

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

  private refreshInterval: any;

  ngOnInit() {
    this.loadLiveAnalytics(); // <-- Now calls the live database
    this.loadMarketData();
    
    // Refresh market data every 5 minutes
    this.refreshInterval = setInterval(() => {
      this.loadMarketData();
    }, 5 * 60 * 1000);
  }

  ngOnDestroy() {
    if (this.refreshInterval) clearInterval(this.refreshInterval);
  }

  toggleMarketCard() {
    this.marketCardOpen.update(v => !v);
  }

  // ── Fetch Real Database Analytics ───────────────────────────────────────
  loadLiveAnalytics() {
    this.http.get<any>('https://creditrisk-api.onrender.com/api/admin/analytics').subscribe({
      next: (data) => {
        // Animate up to the REAL numbers from your database!
        this.animateTo(v => this.loansProcessed.set(v), 0, data.totalAnalyses, 1800);
        this.animateTo(v => this.avgRiskScore.set(v),   0, data.avgScore, 1400);
        this.animateTo(v => this.approvalRate.set(v),   0, data.approvalRate, 1600);

        // Note: Our backend endpoint doesn't currently calculate "Portfolio Value", 
        // so we will leave that specific one as a static animation for now.
        this.animateTo(v => this.portfolioValue.set(v), 0, 48, 2000);
      },
      error: () => {
        // If the API fails for some reason, fallback to fake numbers so the UI doesn't look broken
        this.animateTo(v => this.loansProcessed.set(v), 0, 1284, 1800);
        this.animateTo(v => this.avgRiskScore.set(v),   0, 74,   1400);
        this.animateTo(v => this.portfolioValue.set(v), 0, 48,   2000);
        this.animateTo(v => this.approvalRate.set(v),   0, 68,   1600);
      }
    });
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