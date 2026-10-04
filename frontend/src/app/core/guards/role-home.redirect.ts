import { inject } from '@angular/core';
import { RedirectFunction } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { SessionService } from '../auth/session.service';

export const roleHomeRedirect: RedirectFunction = () => {
  const session = inject(SessionService);

  return session.restore().pipe(
    map((user) => (user ? session.homeUrlFor(user.role) : '/select-user')),
    catchError(() => of('/select-user')),
  );
};
