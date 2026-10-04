import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { SessionService } from '../auth/session.service';

export const authenticatedGuard: CanActivateFn = () => {
  const session = inject(SessionService);
  const router = inject(Router);

  return session.restore().pipe(
    map((user) => user !== null || router.createUrlTree(['/select-user'])),
    catchError(() => of(router.createUrlTree(['/select-user']))),
  );
};
