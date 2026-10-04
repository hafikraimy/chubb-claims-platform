import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { apiErrorInterceptor } from './core/interceptors/api-error.interceptor';
import { demoUserInterceptor } from './core/interceptors/demo-user.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([demoUserInterceptor, apiErrorInterceptor])),
  ],
};
