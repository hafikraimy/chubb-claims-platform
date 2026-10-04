import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { DemoIdentityStore } from './demo-identity-store.service';
import { SessionService } from './session.service';
import { UserRole } from './user-role';

const claimantId = '11111111-1111-1111-1111-111111111111';
const claimant = {
  id: claimantId,
  name: 'Hafiz Claimant',
  role: UserRole.Claimant,
  market: 'MY',
  teamId: null,
};

describe('SessionService', () => {
  let service: SessionService;
  let identityStore: DemoIdentityStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(SessionService);
    identityStore = TestBed.inject(DemoIdentityStore);
    http = TestBed.inject(HttpTestingController);
    identityStore.clear();
  });

  afterEach(() => {
    identityStore.clear();
    http.verify();
  });

  it('stores and validates a selected user', () => {
    service.selectUser(claimantId).subscribe();

    expect(identityStore.read()).toBe(claimantId);
    expect(service.status()).toBe('loading');

    http.expectOne('/api/me').flush(claimant);

    expect(service.currentUser()).toEqual(claimant);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.status()).toBe('authenticated');
  });

  it('restores a stored session', () => {
    identityStore.write(claimantId);
    service.restore().subscribe();

    http.expectOne('/api/me').flush(claimant);

    expect(service.currentUser()).toEqual(claimant);
  });

  it('remains anonymous when there is no stored identity', async () => {
    const user = await firstValueFrom(service.restore());

    expect(user).toBeNull();
    expect(service.status()).toBe('anonymous');
    http.expectNone('/api/me');
  });

  it('clears an invalid stored identity', () => {
    identityStore.write(claimantId);

    service.restore().subscribe({ error: () => undefined });
    http.expectOne('/api/me').flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(identityStore.read()).toBeNull();
    expect(service.currentUser()).toBeNull();
    expect(service.status()).toBe('error');
  });

  it('clears persistent and in-memory session state', () => {
    service.selectUser(claimantId).subscribe();
    http.expectOne('/api/me').flush(claimant);

    service.clear();

    expect(identityStore.read()).toBeNull();
    expect(service.currentUser()).toBeNull();
    expect(service.status()).toBe('anonymous');
  });

  it('maps each role to its home area', () => {
    expect(service.homeUrlFor(UserRole.Claimant)).toBe('/claimant');
    expect(service.homeUrlFor(UserRole.ClaimsOfficer)).toBe('/officer');
    expect(service.homeUrlFor(UserRole.Manager)).toBe('/manager');
  });
});
