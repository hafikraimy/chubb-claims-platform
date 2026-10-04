import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'select-user',
    title: 'Choose demo user',
    loadComponent: () =>
      import('./features/identity/user-selection-page/user-selection-page').then(
        (module) => module.UserSelectionPage,
      ),
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'select-user',
  },
  {
    path: '**',
    redirectTo: 'select-user',
  },
];
