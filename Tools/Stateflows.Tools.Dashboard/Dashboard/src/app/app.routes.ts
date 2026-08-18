import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'overview', pathMatch: 'full' },
  {
    path: 'overview',
    loadComponent: () =>
      import('./pages/overview/overview.component').then((m) => m.OverviewComponent),
  },
  {
    path: 'behaviors/:type/:name',
    loadComponent: () =>
      import('./pages/behavior-detail/behavior-detail.component').then((m) => m.BehaviorDetailComponent),
  },
  { path: '**', redirectTo: 'overview' },
];
