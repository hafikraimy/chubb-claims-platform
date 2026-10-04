import { Routes } from '@angular/router';
import { UserRole } from './core/auth/user-role';
import { authenticatedGuard } from './core/guards/authenticated.guard';
import { roleGuard } from './core/guards/role.guard';
import { roleHomeRedirect } from './core/guards/role-home.redirect';

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
    canActivate: [authenticatedGuard],
    loadComponent: () =>
      import('./core/layout/app-shell/app-shell').then((module) => module.AppShell),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: roleHomeRedirect,
      },
      {
        path: 'claimant',
        canMatch: [roleGuard(UserRole.Claimant)],
        loadChildren: () =>
          import('./features/claimant/claimant.routes').then((module) => module.CLAIMANT_ROUTES),
      },
      {
        path: 'officer',
        canMatch: [roleGuard(UserRole.ClaimsOfficer)],
        loadChildren: () =>
          import('./features/officer/officer.routes').then((module) => module.OFFICER_ROUTES),
      },
      {
        path: 'manager',
        canMatch: [roleGuard(UserRole.Manager)],
        loadChildren: () =>
          import('./features/manager/manager.routes').then((module) => module.MANAGER_ROUTES),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'select-user',
  },
];
