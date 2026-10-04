import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { SessionService } from '../../auth/session.service';
import { UserRole } from '../../auth/user-role';

interface NavigationItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  imports: [
    MatButtonModule,
    MatMenuModule,
    MatSidenavModule,
    MatToolbarModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
  ],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
})
export class AppShell {
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly router = inject(Router);
  private readonly session = inject(SessionService);

  protected readonly currentUser = this.session.currentUser;
  protected readonly homeUrl = computed(() => {
    const user = this.currentUser();
    return user ? this.session.homeUrlFor(user.role) : '/select-user';
  });
  protected readonly mobileNavigationOpen = signal(false);
  protected readonly compactLayout = toSignal(
    this.breakpointObserver.observe('(max-width: 50rem)').pipe(map((result) => result.matches)),
    { initialValue: false },
  );
  protected readonly navigationItems = computed<readonly NavigationItem[]>(() => {
    switch (this.currentUser()?.role) {
      case UserRole.Claimant:
        return [{ label: 'My claims', route: '/claimant/claims' }];
      case UserRole.ClaimsOfficer:
        return [{ label: 'Unassigned queue', route: '/officer/queue' }];
      case UserRole.Manager:
        return [{ label: 'Manager overview', route: '/manager' }];
      default:
        return [];
    }
  });

  protected toggleNavigation(): void {
    this.mobileNavigationOpen.update((open) => !open);
  }

  protected closeMobileNavigation(): void {
    if (this.compactLayout()) {
      this.mobileNavigationOpen.set(false);
    }
  }

  protected switchUser(): void {
    this.session.clear();
    void this.router.navigate(['/select-user']);
  }
}
