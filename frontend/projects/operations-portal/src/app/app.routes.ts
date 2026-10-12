import { Routes } from '@angular/router';
import { DevLogin, roleGuard } from 'shared-ui';

export const routes: Routes = [
  { path: 'login', component: DevLogin, data: { role: 'FraudOfficer', heading: 'FinAI - Operations' } },
  {
    path: '',
    canActivate: [roleGuard('FraudOfficer')],
    children: [
      { path: 'fraud', loadComponent: () => import('./fraud/fraud-feed').then(m => m.FraudFeed) },
      { path: 'dashboard', loadComponent: () => import('./dashboard/dashboard').then(m => m.Dashboard) },
      { path: '', pathMatch: 'full', redirectTo: 'fraud' },
    ],
  },
  { path: '**', redirectTo: '' },
];
