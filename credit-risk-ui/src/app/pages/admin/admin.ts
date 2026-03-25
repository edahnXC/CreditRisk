import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ScrollRevealDirective } from '../../directives/index';

const ADMIN_API = 'https://localhost:7156/api/admin';

@Component({
  selector:    'app-admin',
  standalone:  true,
  imports:     [CommonModule, FormsModule, RouterLink,
                DecimalPipe, DatePipe, ScrollRevealDirective],
  templateUrl: './admin.html',
  styleUrl:    './admin.scss'
})
export class AdminComponent implements OnInit {

  private http = inject(HttpClient);

  // ── Auth ──────────────────────────────────────────────────────────────
  isLoggedIn    = signal(false);
  loginError    = signal('');
  password      = '';

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

  ngOnInit() {
    // Check if already logged in via sessionStorage
    const token = sessionStorage.getItem('admin-token');
    if (token === 'admin-session-token-2024') {
      this.isLoggedIn.set(true);
      this.loadAnalytics();
    }
  }

  // ── Auth ──────────────────────────────────────────────────────────────
  login() {
    this.http.post<any>(`${ADMIN_API}/login`, { password: this.password })
      .subscribe({
        next: (res) => {
          if (res.success) {
            sessionStorage.setItem('admin-token', res.token);
            this.isLoggedIn.set(true);
            this.loginError.set('');
            this.loadAnalytics();
          }
        },
        error: () => {
          this.loginError.set('Incorrect password. Try again.');
        }
      });
  }

  logout() {
    sessionStorage.removeItem('admin-token');
    this.isLoggedIn.set(false);
    this.password = '';
  }

  // ── Section navigation ────────────────────────────────────────────────
  setSection(section: 'analytics' | 'market' | 'learn' | 'logs') {
    this.activeSection.set(section);
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
    }).subscribe({
      next: () => {
        this.marketMessage.set('Market data updated successfully.');
        this.marketSaving.set(false);
      },
      error: () => {
        this.marketMessage.set('Failed to update. Try again.');
        this.marketSaving.set(false);
      }
    });
  }

  refreshLiveData() {
    this.marketSaving.set(true);
    this.marketMessage.set('');
    this.http.post(`${ADMIN_API}/market/refresh`, {}).subscribe({
      next: () => {
        this.marketMessage.set('Live data refreshed successfully.');
        this.marketSaving.set(false);
      },
      error: () => {
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

    this.http.put(`${ADMIN_API}/learn/${item.id}`, item).subscribe({
      next: (updated: any) => {
        this.learnItems.update(items =>
          items.map(i => i.id === updated.id ? updated : i));
        this.editingItem.set(null);
        this.learnMessage.set('Content updated successfully.');
      },
      error: () => {
        this.learnMessage.set('Failed to save. Try again.');
      }
    });
  }

  // ── Logs ──────────────────────────────────────────────────────────────
  loadLogs() {
    this.logsLoading.set(true);
    this.http.get<any[]>(`${ADMIN_API}/logs`).subscribe({
      next:  (data) => { this.logs.set(data); this.logsLoading.set(false); },
      error: ()     => { this.logsLoading.set(false); }
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