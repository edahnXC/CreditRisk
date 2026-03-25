import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ScrollRevealDirective } from '../../directives/index';

const ADMIN_API = 'https://localhost:7156/api/admin';

@Component({
  selector:    'app-learn',
  standalone:  true,
  imports:     [CommonModule, RouterLink, ScrollRevealDirective],
  templateUrl: './learn.html',
  styleUrl:    './learn.scss'
})
export class LearnComponent implements OnInit {

  private http = inject(HttpClient);

  models        = signal<any[]>([]);
  loading       = signal(true);
  activeFilter  = signal('All');
  expandedId    = signal<string | null>(null);

  categories = ['All', 'Risk-Adjusted Return', 'Risk-Return Model',
                'Optimal Bet Sizing', 'Portfolio Loss Metric',
                'Price Process Model', 'Option Pricing Model',
                'Portfolio Optimization', 'Default Distance',
                'Investment Planning', 'Growth Rate',
                'Fund Analysis', 'Comparison Tool'];

  ngOnInit() {
    this.http.get<any[]>(`${ADMIN_API}/learn`).subscribe({
      next:  (data) => { this.models.set(data); this.loading.set(false); },
      error: ()     => {
        this.loading.set(false);
      }
    });
  }

  get filteredModels() {
    const filter = this.activeFilter();
    const items  = this.models();
    return filter === 'All'
      ? items
      : items.filter(m => m.category === filter);
  }

  setFilter(cat: string) {
    this.activeFilter.set(cat);
  }

  toggleExpand(id: string) {
    this.expandedId.update(current => current === id ? null : id);
  }

  isExpanded(id: string): boolean {
    return this.expandedId() === id;
  }

  getCategoryColor(category: string): string {
    const map: Record<string, string> = {
      'Risk-Adjusted Return':  'teal',
      'Risk-Return Model':     'purple',
      'Optimal Bet Sizing':    'amber',
      'Portfolio Loss Metric': 'red',
      'Price Process Model':   'teal',
      'Option Pricing Model':  'coral',
      'Portfolio Optimization':'purple',
      'Default Distance':      'red',
      'Investment Planning':   'teal',
      'Growth Rate':           'amber',
      'Fund Analysis':         'teal',
      'Comparison Tool':       'purple'
    };
    return map[category] ?? 'teal';
  }
}