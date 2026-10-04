import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CurrentUser } from '../../auth/current-user';
import { SessionService } from '../../auth/session.service';
import { UserRole } from '../../auth/user-role';
import { AppShell } from './app-shell';

const manager: CurrentUser = {
  id: '99999999-9999-9999-9999-999999999999',
  name: 'Aisha Manager',
  role: UserRole.Manager,
  market: 'MY',
  teamId: '10000000-0000-0000-0000-000000000001',
};

describe('AppShell', () => {
  const currentUser = signal<CurrentUser | null>(manager);
  const session = {
    currentUser: currentUser.asReadonly(),
    homeUrlFor: vi.fn(() => '/manager'),
    clear: vi.fn(),
  };
  const breakpointObserver = {
    observe: vi.fn(() => of<BreakpointState>({ matches: false, breakpoints: {} })),
  };

  beforeEach(async () => {
    currentUser.set(manager);
    session.clear.mockReset();

    await TestBed.configureTestingModule({
      imports: [AppShell],
      providers: [
        provideRouter([]),
        { provide: SessionService, useValue: session },
        { provide: BreakpointObserver, useValue: breakpointObserver },
      ],
    }).compileComponents();
  });

  it('shows the current user and role navigation', () => {
    const fixture = TestBed.createComponent(AppShell);
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('Aisha Manager');
    expect(content).toContain('Manager overview');
    expect(content).not.toContain('Officer overview');
  });
});
