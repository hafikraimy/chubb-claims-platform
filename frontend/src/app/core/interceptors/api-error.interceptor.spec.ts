import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideRouter, Router } from '@angular/router';
import { ApiError } from '../api/api-error';
import { SessionService } from '../auth/session.service';
import { apiErrorInterceptor } from './api-error.interceptor';

describe('apiErrorInterceptor', () => {
  const snackBar = { open: vi.fn() };
  const session = { clear: vi.fn() };
  let httpClient: HttpClient;
  let http: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBar },
        { provide: SessionService, useValue: session },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    snackBar.open.mockReset();
    session.clear.mockReset();
  });

  afterEach(() => http.verify());

  it('normalizes server errors and shows an accessible snackbar', () => {
    let received: unknown;
    httpClient.get('/api/claims').subscribe({
      error: (error: unknown) => (received = error),
    });

    http.expectOne('/api/claims').flush(
      {
        title: 'Unexpected server error',
        detail: 'The request failed.',
        status: 500,
      },
      { status: 500, statusText: 'Server Error' },
    );

    expect(received).toBeInstanceOf(ApiError);
    expect(snackBar.open).toHaveBeenCalledWith(
      'The request failed.',
      'Dismiss',
      expect.objectContaining({ politeness: 'assertive' }),
    );
  });

  it('leaves validation errors for the requesting form', () => {
    httpClient.post('/api/claims', {}).subscribe({ error: () => undefined });

    http
      .expectOne('/api/claims')
      .flush(
        { title: 'Validation failed', status: 400 },
        { status: 400, statusText: 'Bad Request' },
      );

    expect(snackBar.open).not.toHaveBeenCalled();
  });

  it('clears an expired session and returns to user selection', () => {
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    httpClient.get('/api/claims').subscribe({ error: () => undefined });

    http.expectOne('/api/claims').flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(session.clear).toHaveBeenCalledOnce();
    expect(navigate).toHaveBeenCalledWith(['/select-user']);
  });

  it('does not handle errors from non-API requests', () => {
    let received: unknown;
    httpClient.get('/assets/config.json').subscribe({
      error: (error: unknown) => (received = error),
    });

    http.expectOne('/assets/config.json').flush(null, {
      status: 500,
      statusText: 'Server Error',
    });

    expect(received).not.toBeInstanceOf(ApiError);
    expect(snackBar.open).not.toHaveBeenCalled();
  });
});
