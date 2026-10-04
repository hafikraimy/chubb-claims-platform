import { TestBed } from '@angular/core/testing';
import { PartialMatchRouteSnapshot } from '@angular/router';
import { firstValueFrom, Observable, of } from 'rxjs';
import { CurrentUser } from '../auth/current-user';
import { SessionService } from '../auth/session.service';
import { UserRole } from '../auth/user-role';
import { roleHomeRedirect } from './role-home.redirect';

const manager: CurrentUser = {
  id: '99999999-9999-9999-9999-999999999999',
  name: 'Aisha Manager',
  role: UserRole.Manager,
  market: 'MY',
  teamId: '10000000-0000-0000-0000-000000000001',
};

describe('roleHomeRedirect', () => {
  const session = {
    restore: vi.fn(),
    homeUrlFor: vi.fn(() => '/manager'),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: SessionService, useValue: session }],
    });
    session.restore.mockReset();
    session.homeUrlFor.mockClear();
  });

  async function runRedirect(): Promise<string> {
    const result = TestBed.runInInjectionContext(() =>
      roleHomeRedirect({} as PartialMatchRouteSnapshot),
    );

    return firstValueFrom(result as Observable<string>);
  }

  it("redirects to the current user's home", async () => {
    session.restore.mockReturnValue(of(manager));

    expect(await runRedirect()).toBe('/manager');
  });

  it('redirects an anonymous user to user selection', async () => {
    session.restore.mockReturnValue(of(null));

    expect(await runRedirect()).toBe('/select-user');
  });
});
