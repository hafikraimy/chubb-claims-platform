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
  {
    path: 'my-work',
    title: 'My work',
    loadComponent: () =>
      import('./officer-work-list-page/officer-work-list-page').then(
        (module) => module.OfficerWorkListPage,
      ),
  },
  {
    path: 'claims/:id',
    title: 'Claim workbench',
    loadComponent: () =>
      import('./officer-claim-page/officer-claim-page').then((module) => module.OfficerClaimPage),
  },
];
