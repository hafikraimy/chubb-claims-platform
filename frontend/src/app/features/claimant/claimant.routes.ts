import { Routes } from '@angular/router';

export const CLAIMANT_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'claims',
  },
  {
    path: 'claims',
    title: 'My claims',
    loadComponent: () =>
      import('./claim-list-page/claim-list-page').then((module) => module.ClaimListPage),
  },
  {
    path: 'claims/new',
    title: 'Report an incident',
    loadComponent: () =>
      import('./submit-claim-page/submit-claim-page').then((module) => module.SubmitClaimPage),
  },
  {
    path: 'claims/:id',
    title: 'Claim details',
    loadComponent: () =>
      import('./claim-detail-page/claim-detail-page').then((module) => module.ClaimDetailPage),
  },
];
