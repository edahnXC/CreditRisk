import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const API        = 'https://localhost:7156/api/finance';
const MARKET_API = 'https://localhost:7156/api/market';

@Injectable({ providedIn: 'root' })
export class FinanceService {
  constructor(private http: HttpClient) {}

  // ── Finance Engine endpoints ──────────────────────────────────────────
  calculateSharpe(data: any):       Observable<any> { return this.http.post(`${API}/sharpe`,           data); }
  calculateCAPM(data: any):         Observable<any> { return this.http.post(`${API}/capm`,             data); }
  calculateKelly(data: any):        Observable<any> { return this.http.post(`${API}/kelly`,            data); }
  calculateVaR(data: any):          Observable<any> { return this.http.post(`${API}/var`,              data); }
  calculateGBM(data: any):          Observable<any> { return this.http.post(`${API}/gbm`,              data); }
  calculateBlackScholes(data: any): Observable<any> { return this.http.post(`${API}/blackscholes`,     data); }
  optimizePortfolio(data: any):     Observable<any> { return this.http.post(`${API}/portfolio`,        data); }
  calculateMerton(data: any):       Observable<any> { return this.http.post(`${API}/merton`,           data); }
  calculateSIP(data: any):          Observable<any> { return this.http.post(`${API}/sip`,              data); }
  calculateCAGR(data: any):         Observable<any> { return this.http.post(`${API}/cagr`,             data); }
  trackNAV(data: any):              Observable<any> { return this.http.post(`${API}/nav`,              data); }
  compareSipVsLoan(data: any):      Observable<any> { return this.http.post(`${API}/sip-vs-loan`,      data); }
  getLoanRiskProfile(data: any):    Observable<any> { return this.http.post(`${API}/loan-risk-profile`,data); }

  // ── Market Data endpoints ─────────────────────────────────────────────
  getMarketSnapshot(): Observable<any> {
    return this.http.get(`${MARKET_API}/snapshot`);
  }

  updateManualMarketValues(data: {
    repoRate?:      number;
    inflationRate?: number;
    goldPer10g?:    number;
  }): Observable<any> {
    return this.http.post(`${MARKET_API}/update-manual`, data);
  }

  forceRefreshMarketData(): Observable<any> {
    return this.http.post(`${MARKET_API}/refresh`, {});
  }
}