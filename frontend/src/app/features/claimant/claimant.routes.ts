import { Routes } from '@angular/router';

export const CLAIMANT_ROUTES: Routes = [
  {
    path: '',
    title: 'Claimant overview',
    loadComponent: () =>
      import('./claimant-home-page/claimant-home-page').then((module) => module.ClaimantHomePage),
  },
];
