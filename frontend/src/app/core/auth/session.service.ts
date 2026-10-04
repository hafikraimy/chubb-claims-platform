import { computed, inject, Service, signal } from '@angular/core';
import { catchError, Observable, of, tap, throwError } from 'rxjs';
import { CurrentUserApiService } from '../api/current-user-api.service';
import { CurrentUser } from './current-user';
import { DemoIdentityStore } from './demo-identity-store.service';

export type SessionStatus = 'anonymous' | 'loading' | 'authenticated' | 'error';

@Service()
export class SessionService {
  private readonly currentUserApi = inject(CurrentUserApiService);
  private readonly identityStore = inject(DemoIdentityStore);
  private readonly currentUserState = signal<CurrentUser | null>(null);
  private readonly statusState = signal<SessionStatus>('anonymous');

  readonly currentUser = this.currentUserState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserState() !== null);

  selectUser(userId: string): Observable<CurrentUser> {
    this.identityStore.write(userId);
    return this.loadCurrentUser();
  }

  restore(): Observable<CurrentUser | null> {
    const currentUser = this.currentUserState();

    if (currentUser) {
      return of(currentUser);
    }

    if (!this.identityStore.read()) {
      this.statusState.set('anonymous');
      return of(null);
    }

    return this.loadCurrentUser();
  }

  clear(): void {
    this.identityStore.clear();
    this.currentUserState.set(null);
    this.statusState.set('anonymous');
  }

  private loadCurrentUser(): Observable<CurrentUser> {
    this.statusState.set('loading');

    return this.currentUserApi.getCurrentUser().pipe(
      tap((user) => {
        this.currentUserState.set(user);
        this.statusState.set('authenticated');
      }),
      catchError((error: unknown) => {
        this.identityStore.clear();
        this.currentUserState.set(null);
        this.statusState.set('error');

        return throwError(() => error);
      }),
    );
  }
}
