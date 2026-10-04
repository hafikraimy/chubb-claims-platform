import { Routes } from '@angular/router';

export const OFFICER_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'queue',
  },
  {
    path: 'queue',
    title: 'Unassigned queue',
    loadComponent: () =>
      import('./officer-queue-page/officer-queue-page').then((module) => module.OfficerQueuePage),
  },
];
