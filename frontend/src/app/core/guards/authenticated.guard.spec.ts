import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { firstValueFrom, Observable, of, throwError } from 'rxjs';
import { CurrentUser } from '../auth/current-user';
import { SessionService } from '../auth/session.service';
import { UserRole } from '../auth/user-role';
import { authenticatedGuard } from './authenticated.guard';

const claimant: CurrentUser = {
  id: '11111111-1111-1111-1111-111111111111',
  name: 'Hafiz Claimant',
  role: UserRole.Claimant,
  market: 'MY',
  teamId: null,
};

describe('authenticatedGuard', () => {
  const session = {
    restore: vi.fn(),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: SessionService, useValue: session }],
    });
    session.restore.mockReset();
  });

  async function runGuard(): Promise<boolean | UrlTree> {
    const result = TestBed.runInInjectionContext(() =>
      authenticatedGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );

    return firstValueFrom(result as Observable<boolean | UrlTree>);
  }

  it('allows a restored session', async () => {
    session.restore.mockReturnValue(of(claimant));

    expect(await runGuard()).toBe(true);
  });

  it('redirects an anonymous session to user selection', async () => {
    session.restore.mockReturnValue(of(null));

    expect(((await runGuard()) as UrlTree).toString()).toBe('/select-user');
  });

  it('redirects when session restoration fails', async () => {
    session.restore.mockReturnValue(throwError(() => new Error('failed')));

    expect(((await runGuard()) as UrlTree).toString()).toBe('/select-user');
  });
});
