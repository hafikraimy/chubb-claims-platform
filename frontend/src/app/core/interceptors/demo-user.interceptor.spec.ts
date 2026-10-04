import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DemoIdentityStore } from '../auth/demo-identity-store.service';
import { demoUserInterceptor } from './demo-user.interceptor';

describe('demoUserInterceptor', () => {
  let httpClient: HttpClient;
  let http: HttpTestingController;
  let identityStore: DemoIdentityStore;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([demoUserInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    identityStore = TestBed.inject(DemoIdentityStore);
    identityStore.clear();
  });

  afterEach(() => {
    identityStore.clear();
    http.verify();
  });

  it('adds the selected demo user to API requests', () => {
    identityStore.write('11111111-1111-1111-1111-111111111111');

    httpClient.get('/api/me').subscribe();

    const request = http.expectOne('/api/me');
    expect(request.request.headers.get('X-Demo-User')).toBe('11111111-1111-1111-1111-111111111111');
    request.flush({});
  });

  it('does not add a header when no user is selected', () => {
    httpClient.get('/api/me').subscribe();

    const request = http.expectOne('/api/me');
    expect(request.request.headers.has('X-Demo-User')).toBe(false);
    request.flush({});
  });

  it('does not add the demo header to external requests', () => {
    identityStore.write('11111111-1111-1111-1111-111111111111');

    httpClient.get('https://example.com/config').subscribe();

    const request = http.expectOne('https://example.com/config');
    expect(request.request.headers.has('X-Demo-User')).toBe(false);
    request.flush({});
  });
});
