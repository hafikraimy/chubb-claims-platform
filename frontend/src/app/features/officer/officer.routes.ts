import { Routes } from '@angular/router';

export const OFFICER_ROUTES: Routes = [
  {
    path: '',
    title: 'Officer overview',
    loadComponent: () =>
      import('./officer-home-page/officer-home-page').then((module) => module.OfficerHomePage),
  },
];
