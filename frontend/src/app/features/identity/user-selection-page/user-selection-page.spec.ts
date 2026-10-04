import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DemoIdentityStore } from '../../../core/auth/demo-identity-store.service';
import { UserRole } from '../../../core/auth/user-role';
import { demoUserInterceptor } from '../../../core/interceptors/demo-user.interceptor';
import { UserSelectionPage } from './user-selection-page';

describe('UserSelectionPage', () => {
  let http: HttpTestingController;
  let identityStore: DemoIdentityStore;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserSelectionPage],
      providers: [
        provideHttpClient(withInterceptors([demoUserInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    identityStore = TestBed.inject(DemoIdentityStore);
    identityStore.clear();
  });

  afterEach(() => {
    identityStore.clear();
    http.verify();
  });

  it('renders all seeded demo users', () => {
    const fixture = TestBed.createComponent(UserSelectionPage);
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('mat-card');
    expect(cards).toHaveLength(4);
    expect(fixture.nativeElement.textContent).toContain('Hafiz Claimant');
    expect(fixture.nativeElement.textContent).toContain('Aisha Manager');
  });

  it('selects and validates a demo user', () => {
    const fixture = TestBed.createComponent(UserSelectionPage);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector(
      'button[aria-label="Continue as Hafiz Claimant"]',
    ) as HTMLButtonElement;
    button.click();

    const request = http.expectOne('/api/me');
    expect(request.request.headers.get('X-Demo-User')).toBe('11111111-1111-1111-1111-111111111111');
    request.flush({
      id: '11111111-1111-1111-1111-111111111111',
      name: 'Hafiz Claimant',
      role: UserRole.Claimant,
      market: 'MY',
      teamId: null,
    });
    fixture.detectChanges();

    const status = fixture.nativeElement.querySelector('[role="status"]');
    expect(status.textContent).toContain('Signed in as');
    expect(status.textContent).toContain('Hafiz Claimant');
  });

  it('shows a useful message when the API is unavailable', () => {
    const fixture = TestBed.createComponent(UserSelectionPage);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector(
      'button[aria-label="Continue as Hafiz Claimant"]',
    ) as HTMLButtonElement;
    button.click();

    http.expectOne('/api/me').flush(null, {
      status: 503,
      statusText: 'Service Unavailable',
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'Check that the API is running',
    );
  });
});
