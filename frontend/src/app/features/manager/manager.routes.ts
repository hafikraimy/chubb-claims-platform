import { Routes } from '@angular/router';

export const MANAGER_ROUTES: Routes = [
  {
    path: '',
    title: 'Manager overview',
    loadComponent: () =>
      import('./manager-home-page/manager-home-page').then((module) => module.ManagerHomePage),
  },
];
