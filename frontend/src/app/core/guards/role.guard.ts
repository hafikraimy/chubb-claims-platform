import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { SessionService } from '../auth/session.service';
import { UserRole } from '../auth/user-role';

export function roleGuard(requiredRole: UserRole): CanMatchFn {
  return () => {
    const session = inject(SessionService);
    const router = inject(Router);

    return session.restore().pipe(
      map((user) => {
        if (!user) {
          return router.createUrlTree(['/select-user']);
        }

        return user.role === requiredRole ? true : router.parseUrl(session.homeUrlFor(user.role));
      }),
      catchError(() => of(router.createUrlTree(['/select-user']))),
    );
  };
}
