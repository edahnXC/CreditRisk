import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home/home').then(m => m.HomeComponent)
  },
  {
    path: 'apply',
    loadComponent: () =>
      import('./pages/apply/apply').then(m => m.ApplyComponent)
  },
  {
    path: 'invest',
    loadComponent: () =>
      import('./pages/invest/invest').then(m => m.InvestComponent)
  },
  {
    path: 'compare',
    loadComponent: () =>
      import('./pages/compare/compare').then(m => m.CompareComponent)
  },
  {
    path: 'admin',
    loadComponent: () =>
      import('./pages/admin/admin').then(m => m.AdminComponent)
  },
  {
    path: 'loans/:id',
    loadComponent: () =>
      import('./pages/loan-detail/loan-detail').then(m => m.LoanDetailComponent)
  },
  {
    path: 'learn',
    loadComponent: () =>
      import('./pages/learn/learn').then(m => m.LearnComponent)
  },
  // The wildcard catch-all MUST go last
  {
    path: '**',
    redirectTo: ''
  }
];