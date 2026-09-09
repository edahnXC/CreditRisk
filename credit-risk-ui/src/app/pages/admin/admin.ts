import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ScrollRevealDirective } from '../../directives/index';

const ADMIN_API = 'https://creditrisk-api.onrender.com/api/admin';

@Component({
  selector:    'app-admin',
  standalone:  true,
  imports:     [CommonModule, FormsModule, RouterLink,
                DecimalPipe, DatePipe, ScrollRevealDirective],
  templateUrl: './admin.html',
  styleUrl:    './admin.scss'
})
export class AdminComponent implements OnInit, OnDestroy {

  private http = inject(HttpClient);

  // ── Auth & Session ────────────────────────────────────────────────────
  isLoggedIn        = signal(false);
  loginError        = signal('');
  loginLoading      = signal(false);
  sessionRemaining  = signal('');
  password          = '';
  private sessionTimer: any = null;

  // ── Active section ────────────────────────────────────────────────────
  activeSection = signal<'analytics' | 'market' | 'learn' | 'logs'>('analytics');

  // ── Analytics ─────────────────────────────────────────────────────────
  analytics     = signal<any>(null);
  analyticsLoading = signal(false);

  // ── Market ────────────────────────────────────────────────────────────
  marketForm = {
    repoRate:      6.50,
    inflationRate: 4.85,
    goldPer10g:    87000
  };
  marketSaving  = signal(false);
  marketMessage = signal('');

  // ── Learn ─────────────────────────────────────────────────────────────
  learnItems    = signal<any[]>([]);
  learnLoading  = signal(false);
  editingItem   = signal<any>(null);
  learnMessage  = signal('');

  // ── Logs ──────────────────────────────────────────────────────────────
  logs          = signal<any[]>([]);
  logsLoading   = signal(false);

  private getAuthOptions() {
    const token = sessionStorage.getItem('admin-token') || '';
    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${token}`
      })
    };
  }

  private handleAuthError(err: any) {
    if (err?.status === 401 || err?.status === 403) {
      this.logout();
      this.loginError.set('Session expired or unauthorized. Please sign in again.');
    }
  }

  ngOnInit() {
    // Check if a JWT token already exists in sessionStorage
    const token = sessionStorage.getItem('admin-token');
    if (token) {
      // Cryptographically verify with backend
      this.http.get<any>(`${ADMIN_API}/verify`, this.getAuthOptions()).subscribe({
        next: () => {
          this.isLoggedIn.set(true);
          this.startSessionTimer();
          this.loadAnalytics();
        },
        error: () => {
          // Token is invalid, expired, or tampered with
          this.logout();
        }
      });
    }
  }

  ngOnDestroy() {
    this.stopSessionTimer();
  }

  // ── Auth ──────────────────────────────────────────────────────────────
  login() {
    if (!this.password.trim()) {
      this.loginError.set('Please enter the admin password.');
      return;
    }

    this.loginLoading.set(true);
    this.loginError.set('');

    this.http.post<any>(`${ADMIN_API}/login`, { password: this.password })
      .subscribe({
        next: (res) => {
          this.loginLoading.set(false);
          if (res?.success && res?.token) {
            sessionStorage.setItem('admin-token', res.token);
            if (res.expiresAt) {
              sessionStorage.setItem('admin-token-expires', res.expiresAt);
            } else {
              // Default to 8-hour window from current time
              const fallbackExp = new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString();
              sessionStorage.setItem('admin-token-expires', fallbackExp);
            }
            this.isLoggedIn.set(true);
            this.startSessionTimer();
            this.loginError.set('');
            this.password = '';
            this.loadAnalytics();
          } else {
            this.loginError.set(res?.message || 'Authentication failed.');
          }
        },
        error: (err) => {
          this.loginLoading.set(false);
          this.loginError.set(err?.error?.message || 'Incorrect password. Try again.');
        }
      });
  }

  logout() {
    this.stopSessionTimer();
    sessionStorage.removeItem('admin-token');
    sessionStorage.removeItem('admin-token-expires');
    this.isLoggedIn.set(false);
    this.sessionRemaining.set('');
    this.password = '';
  }

  private startSessionTimer() {
    this.stopSessionTimer();
    this.updateSessionCountdown();
    this.sessionTimer = setInterval(() => {
      this.updateSessionCountdown();
    }, 15000); // Recalculate every 15 seconds
  }

  private stopSessionTimer() {
    if (this.sessionTimer) {
      clearInterval(this.sessionTimer);
      this.sessionTimer = null;
    }
  }

  private updateSessionCountdown() {
    const expiresStr = sessionStorage.getItem('admin-token-expires');
    if (!expiresStr) {
      this.sessionRemaining.set('Active');
      return;
    }

    const expiresAt = new Date(expiresStr).getTime();
    const now = Date.now();
    const diffMs = expiresAt - now;

    if (diffMs <= 0) {
      this.logout();
      this.loginError.set('Your 8-hour administrator session has expired. Please sign in again.');
      return;
    }

    const totalMinutes = Math.floor(diffMs / (1000 * 60));
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours > 0) {
      this.sessionRemaining.set(`${hours}h ${minutes}m remaining`);
    } else {
      this.sessionRemaining.set(`${minutes}m remaining`);
    }
  }

  // ── Section navigation ────────────────────────────────────────────────
  setSection(section: 'analytics' | 'market' | 'learn' | 'logs') {
    this.activeSection.set(section);
    window.scrollTo({ top: 0, behavior: 'smooth' });
    switch (section) {
      case 'analytics': this.loadAnalytics(); break;
      case 'learn':     this.loadLearnContent(); break;
      case 'logs':      this.loadLogs(); break;
    }
  }

  // ── Analytics ─────────────────────────────────────────────────────────
  loadAnalytics() {
    this.analyticsLoading.set(true);
    this.http.get<any>(`${ADMIN_API}/analytics`).subscribe({
      next:  (data) => { this.analytics.set(data); this.analyticsLoading.set(false); },
      error: ()     => { this.analyticsLoading.set(false); }
    });
  }

  // ── Market ────────────────────────────────────────────────────────────
  saveMarketData() {
    this.marketSaving.set(true);
    this.marketMessage.set('');
    this.http.post(`${ADMIN_API}/market/update`, {
      repoRate:      this.marketForm.repoRate,
      inflationRate: this.marketForm.inflationRate,
      goldPer10g:    this.marketForm.goldPer10g
    }, this.getAuthOptions()).subscribe({
      next: () => {
        this.marketMessage.set('Market data updated successfully.');
        this.marketSaving.set(false);
      },
      error: (err) => {
        this.handleAuthError(err);
        this.marketMessage.set('Failed to update. Try again.');
        this.marketSaving.set(false);
      }
    });
  }

  refreshLiveData() {
    this.marketSaving.set(true);
    this.marketMessage.set('');
    this.http.post(`${ADMIN_API}/market/refresh`, {}, this.getAuthOptions()).subscribe({
      next: () => {
        this.marketMessage.set('Live data refreshed successfully.');
        this.marketSaving.set(false);
      },
      error: (err) => {
        this.handleAuthError(err);
        this.marketMessage.set('Refresh failed. Try again.');
        this.marketSaving.set(false);
      }
    });
  }

  // ── Learn ─────────────────────────────────────────────────────────────
  loadLearnContent() {
    this.learnLoading.set(true);
    this.http.get<any[]>(`${ADMIN_API}/learn`).subscribe({
      next:  (data) => { this.learnItems.set(data); this.learnLoading.set(false); },
      error: ()     => { this.learnLoading.set(false); }
    });
  }

  startEdit(item: any) {
    this.editingItem.set({ ...item });
    this.learnMessage.set('');
  }

  cancelEdit() {
    this.editingItem.set(null);
  }

  saveLearnItem() {
    const item = this.editingItem();
    if (!item) return;

    this.http.put(`${ADMIN_API}/learn/${item.id}`, item, this.getAuthOptions()).subscribe({
      next: (updated: any) => {
        this.learnItems.update(items =>
          items.map(i => i.id === updated.id ? updated : i));
        this.editingItem.set(null);
        this.learnMessage.set('Content updated successfully.');
      },
      error: (err) => {
        this.handleAuthError(err);
        this.learnMessage.set('Failed to save. Try again.');
      }
    });
  }

  // ── Logs ──────────────────────────────────────────────────────────────
  loadLogs() {
    this.logsLoading.set(true);
    this.http.get<any[]>(`${ADMIN_API}/logs`, this.getAuthOptions()).subscribe({
      next:  (data) => { this.logs.set(data); this.logsLoading.set(false); },
      error: (err)  => {
        this.handleAuthError(err);
        this.logsLoading.set(false);
      }
    });
  }

  getDecisionClass(decision: string): string {
    if (decision === 'Approve') return 'approve';
    if (decision === 'Review')  return 'review';
    return 'reject';
  }

  formatCurrency(value: number): string {
    if (value >= 10000000) return '₹' + (value / 10000000).toFixed(1) + 'Cr';
    if (value >= 100000)   return '₹' + (value / 100000).toFixed(1) + 'L';
    return '₹' + Math.round(value).toLocaleString('en-IN');
  }
}