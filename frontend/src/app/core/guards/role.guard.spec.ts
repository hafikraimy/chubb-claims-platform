import { TestBed } from '@angular/core/testing';
import {
  PartialMatchRouteSnapshot,
  provideRouter,
  Route,
  UrlSegment,
  UrlTree,
} from '@angular/router';
import { firstValueFrom, Observable, of } from 'rxjs';
import { CurrentUser } from '../auth/current-user';
import { SessionService } from '../auth/session.service';
import { UserRole } from '../auth/user-role';
import { roleGuard } from './role.guard';

const officer: CurrentUser = {
  id: '22222222-2222-2222-2222-222222222222',
  name: 'Ben Claims Officer',
  role: UserRole.ClaimsOfficer,
  market: 'MY',
  teamId: '10000000-0000-0000-0000-000000000001',
};

describe('roleGuard', () => {
  const session = {
    restore: vi.fn(),
    homeUrlFor: vi.fn(() => '/officer'),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: SessionService, useValue: session }],
    });
    session.restore.mockReset();
    session.homeUrlFor.mockClear();
  });

  async function runGuard(requiredRole: UserRole): Promise<boolean | UrlTree> {
    const guard = roleGuard(requiredRole);
    const result = TestBed.runInInjectionContext(() =>
      guard({} as Route, [] as UrlSegment[], {} as PartialMatchRouteSnapshot),
    );

    return firstValueFrom(result as Observable<boolean | UrlTree>);
  }

  it('allows the required role', async () => {
    session.restore.mockReturnValue(of(officer));

    expect(await runGuard(UserRole.ClaimsOfficer)).toBe(true);
  });

  it("redirects a different role to that user's home", async () => {
    session.restore.mockReturnValue(of(officer));

    expect(((await runGuard(UserRole.Manager)) as UrlTree).toString()).toBe('/officer');
    expect(session.homeUrlFor).toHaveBeenCalledWith(UserRole.ClaimsOfficer);
  });

  it('redirects an anonymous user to user selection', async () => {
    session.restore.mockReturnValue(of(null));

    expect(((await runGuard(UserRole.Manager)) as UrlTree).toString()).toBe('/select-user');
  });
});
