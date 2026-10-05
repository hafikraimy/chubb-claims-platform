import { Routes } from '@angular/router';

export const MANAGER_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'Manager overview',
    loadComponent: () =>
      import('./manager-home-page/manager-home-page').then((module) => module.ManagerHomePage),
  },
  {
    path: 'claims/:id',
    title: 'Manager claim detail',
    loadComponent: () =>
      import('./manager-claim-detail-page/manager-claim-detail-page').then(
        (module) => module.ManagerClaimDetailPage,
      ),
  },
];
