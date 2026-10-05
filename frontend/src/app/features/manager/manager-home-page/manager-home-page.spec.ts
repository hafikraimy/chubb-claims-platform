import { TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ManagerApiService } from '../../../core/api/manager-api.service';
import { ClaimStatus, ClaimType } from '../../../shared/models/claim.models';
import { ManagerDashboard } from '../../../shared/models/work-management.models';
import { ManagerHomePage } from './manager-home-page';

const dashboard: ManagerDashboard = {
  claims: [
    {
      id: 'claim-1',
      referenceNumber: 'MY-2026-001',
      type: ClaimType.Motor,
      market: 'MY',
      currency: 'MYR',
      incidentDate: '2026-10-01',
      reportedLossAmount: 2500,
      assessedLossAmount: null,
      status: ClaimStatus.Submitted,
      assignedOfficerId: null,
      submittedAt: '2026-10-01T00:00:00Z',
      updatedAt: '2026-10-01T00:00:00Z',
    },
  ],
  officers: [
    {
      officerId: 'officer-1',
      officerName: 'Aisha Tan',
      openClaimCount: 2,
      inReviewCount: 1,
      awaitingInfoCount: 1,
      averageOpenClaimAgeDays: 2,
      oldestOpenClaimAgeDays: 3,
    },
  ],
  exposure: [{ currency: 'MYR', amount: 2500 }],
  performance: {
    periodStart: '2026-09-05T00:00:00Z',
    periodEnd: '2026-10-05T00:00:00Z',
    totalDecisions: 3,
    averageDecisionHours: 12,
    officers: [
      {
        officerId: 'officer-1',
        officerName: 'Aisha Tan',
        settledCount: 2,
        rejectedCount: 1,
        totalDecisions: 3,
        averageDecisionHours: 12,
      },
    ],
  },
};

describe('ManagerHomePage', () => {
  const managerApi = { getDashboard: vi.fn(), assignClaim: vi.fn() };
  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [ManagerHomePage],
      providers: [{ provide: ManagerApiService, useValue: managerApi }],
    }).compileComponents();
  });

  it('shows loading while requesting dashboard data', () => {
    managerApi.getDashboard.mockReturnValue(new Subject<ManagerDashboard>());
    const fixture = TestBed.createComponent(ManagerHomePage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Loading manager dashboard');
  });

  it('renders exposure, workload, claims, and performance', () => {
    managerApi.getDashboard.mockReturnValue(of(dashboard));
    const fixture = TestBed.createComponent(ManagerHomePage);
    fixture.detectChanges();
    const content = fixture.nativeElement.textContent;
    expect(content).toContain('MY-2026-001');
    expect(content).toContain('Aisha Tan');
    expect(content).toContain('Exposure');
    expect(content).toContain('Team performance');
  });

  it('selects the currently assigned officer when the dashboard loads', () => {
    managerApi.getDashboard.mockReturnValue(
      of({
        ...dashboard,
        claims: [
          {
            ...dashboard.claims[0],
            status: ClaimStatus.InReview,
            assignedOfficerId: 'officer-1',
          },
        ],
      }),
    );
    const fixture = TestBed.createComponent(ManagerHomePage);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;

    expect(select.value).toBe('officer-1');
    expect(select.selectedOptions[0].textContent).toContain('Aisha Tan');
  });

  it('assigns a selected officer and refreshes the dashboard', () => {
    managerApi.getDashboard.mockReturnValue(of(dashboard));
    managerApi.assignClaim.mockReturnValue(
      of({
        claimId: 'claim-1',
        assignedOfficerId: 'officer-1',
        status: ClaimStatus.InReview,
        updatedAt: '2026-10-05T00:00:00Z',
      }),
    );
    const fixture = TestBed.createComponent(ManagerHomePage);
    fixture.detectChanges();
    fixture.componentInstance['selections'].set({ 'claim-1': 'officer-1' });
    fixture.componentInstance['assign'](dashboard.claims[0]);
    fixture.detectChanges();
    expect(managerApi.assignClaim).toHaveBeenCalledWith('claim-1', { officerId: 'officer-1' });
    expect(managerApi.getDashboard).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('assignment updated');
  });

  it('shows assignment errors without losing the dashboard', () => {
    managerApi.getDashboard.mockReturnValue(of(dashboard));
    managerApi.assignClaim.mockReturnValue(
      throwError(() => new ApiError(409, 'Conflict', 'Claim changed.', {}, null)),
    );
    const fixture = TestBed.createComponent(ManagerHomePage);
    fixture.detectChanges();
    fixture.componentInstance['selections'].set({ 'claim-1': 'officer-1' });
    fixture.componentInstance['assign'](dashboard.claims[0]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Claim changed.');
    expect(fixture.nativeElement.textContent).toContain('MY-2026-001');
  });
});
