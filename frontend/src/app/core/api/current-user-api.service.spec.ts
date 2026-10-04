import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UserRole } from '../auth/user-role';
import { CurrentUserApiService } from './current-user-api.service';

describe('CurrentUserApiService', () => {
  it('loads the current user from the API', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    const service = TestBed.inject(CurrentUserApiService);
    const http = TestBed.inject(HttpTestingController);

    service.getCurrentUser().subscribe((user) => {
      expect(user.name).toBe('Hafiz Claimant');
      expect(user.role).toBe(UserRole.Claimant);
    });

    const request = http.expectOne('/api/me');
    expect(request.request.method).toBe('GET');
    request.flush({
      id: '11111111-1111-1111-1111-111111111111',
      name: 'Hafiz Claimant',
      role: UserRole.Claimant,
      market: 'MY',
      teamId: null,
    });

    http.verify();
  });
});
