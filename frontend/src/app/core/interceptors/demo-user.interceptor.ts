import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { DemoIdentityStore } from '../auth/demo-identity-store.service';

export const demoUserInterceptor: HttpInterceptorFn = (request, next) => {
  const selectedUserId = inject(DemoIdentityStore).read();

  if (!selectedUserId || !request.url.startsWith('/api/')) {
    return next(request);
  }

  return next(
    request.clone({
      setHeaders: {
        'X-Demo-User': selectedUserId,
      },
    }),
  );
};
