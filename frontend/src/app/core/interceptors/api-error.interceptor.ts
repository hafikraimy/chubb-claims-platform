import { HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ApiErrorService } from '../api/api-error.service';
import { SessionService } from '../auth/session.service';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/')) {
    return next(request);
  }

  const errorService = inject(ApiErrorService);
  const injector = inject(Injector);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  return next(request).pipe(
    catchError((error: unknown) => {
      const apiError = errorService.normalize(error);

      if (apiError.status === 401) {
        injector.get(SessionService).clear();
        void router.navigate(['/select-user']);
      } else if (apiError.status === 0 || apiError.status === 403 || apiError.status >= 500) {
        snackBar.open(apiError.detail, 'Dismiss', {
          duration: 6000,
          politeness: 'assertive',
        });
      }

      return throwError(() => apiError);
    }),
  );
};
