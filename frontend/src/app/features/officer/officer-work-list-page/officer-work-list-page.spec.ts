import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { OfficerWorkApiService } from '../../../core/api/officer-work-api.service';
import { ClaimStatus, ClaimType } from '../../../shared/models/claim.models';
import { WorkClaimSummary } from '../../../shared/models/work-management.models';
import { OfficerWorkListPage } from './officer-work-list-page';

const claim: WorkClaimSummary = {
  id: 'claim-1',
  referenceNumber: 'MY-2026-001',
  type: ClaimType.Motor,
  market: 'MY',
  currency: 'MYR',
  incidentDate: '2026-10-01',
  reportedLossAmount: 2500,
  assessedLossAmount: 2200,
  status: ClaimStatus.InReview,
  assignedOfficerId: 'officer-my-1',
  submittedAt: '2026-10-01T01:30:00Z',
  updatedAt: '2026-10-02T02:45:00Z',
};

describe('OfficerWorkListPage', () => {
  const officerWorkApi = {
    getMyClaims: vi.fn(),
  };

  beforeEach(async () => {
    officerWorkApi.getMyClaims.mockReset();

    await TestBed.configureTestingModule({
      imports: [OfficerWorkListPage],
      providers: [provideRouter([]), { provide: OfficerWorkApiService, useValue: officerWorkApi }],
    }).compileComponents();
  });

  it('shows a loading state while assigned claims are requested', () => {
    officerWorkApi.getMyClaims.mockReturnValue(new Subject<WorkClaimSummary[]>());

    const fixture = TestBed.createComponent(OfficerWorkListPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading your assigned claims');
  });

  it('renders assigned claims and their assessment state', () => {
    officerWorkApi.getMyClaims.mockReturnValue(of([claim]));

    const fixture = TestBed.createComponent(OfficerWorkListPage);
    fixture.detectChanges();

    const table = fixture.nativeElement.querySelector('table') as HTMLTableElement;
    expect(table.textContent).toContain('MY-2026-001');
    expect(table.textContent).toContain('In review');
    expect(table.textContent).toContain('2,200.00');
  });

  it('identifies a claim without a recorded assessment', () => {
    officerWorkApi.getMyClaims.mockReturnValue(of([{ ...claim, assessedLossAmount: null }]));

    const fixture = TestBed.createComponent(OfficerWorkListPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('table').textContent).toContain('Not recorded');
  });

  it('shows an empty state linked to the unassigned queue', () => {
    officerWorkApi.getMyClaims.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(OfficerWorkListPage);
    fixture.detectChanges();

    const emptyState = fixture.nativeElement.querySelector('app-empty-state') as HTMLElement;
    const queueLink = emptyState.querySelector('a') as HTMLAnchorElement;
    expect(emptyState.textContent).toContain('No assigned claims');
    expect(queueLink.getAttribute('href')).toBe('/officer/queue');
  });

  it('shows an API error and retries the request', () => {
    const error = new ApiError(500, 'Failed', 'Please try later.', {}, null);
    officerWorkApi.getMyClaims
      .mockReturnValueOnce(throwError(() => error))
      .mockReturnValueOnce(of([claim]));

    const fixture = TestBed.createComponent(OfficerWorkListPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Please try later.');

    const retry = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    retry.click();
    fixture.detectChanges();

    expect(officerWorkApi.getMyClaims).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('MY-2026-001');
  });
});
