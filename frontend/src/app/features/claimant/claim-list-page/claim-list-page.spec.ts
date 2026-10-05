import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { ClaimStatus, ClaimSummary, ClaimType } from '../../../shared/models/claim.models';
import { ClaimListPage } from './claim-list-page';

const claim: ClaimSummary = {
  id: 'claim-1',
  referenceNumber: 'MY-2026-001',
  type: ClaimType.Motor,
  incidentDate: '2026-10-01',
  reportedLossAmount: 2500,
  currency: 'MYR',
  status: ClaimStatus.InReview,
  submittedAt: '2026-10-01T01:30:00Z',
};

describe('ClaimListPage', () => {
  const claimsApi = {
    getClaims: vi.fn(),
  };

  beforeEach(async () => {
    claimsApi.getClaims.mockReset();

    await TestBed.configureTestingModule({
      imports: [ClaimListPage],
      providers: [provideRouter([]), { provide: ClaimsApiService, useValue: claimsApi }],
    }).compileComponents();
  });

  it('shows a loading state while claims are requested', () => {
    claimsApi.getClaims.mockReturnValue(new Subject<ClaimSummary[]>());

    const fixture = TestBed.createComponent(ClaimListPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading your claims');
  });

  it('renders returned claims with a detail link and status', () => {
    claimsApi.getClaims.mockReturnValue(of([claim]));

    const fixture = TestBed.createComponent(ClaimListPage);
    fixture.detectChanges();

    const link = fixture.nativeElement.querySelector('table a') as HTMLAnchorElement;
    expect(link.textContent).toContain('MY-2026-001');
    expect(link.getAttribute('href')).toBe('/claimant/claims/claim-1');
    expect(fixture.nativeElement.textContent).toContain('In review');
    expect(fixture.nativeElement.textContent).toContain('MYR');
    expect(fixture.nativeElement.textContent).not.toContain('›');
  });

  it('shows an empty state when the claimant has no claims', () => {
    claimsApi.getClaims.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(ClaimListPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No claims yet');
    expect(fixture.nativeElement.querySelectorAll('a').length).toBe(1);
  });

  it('shows the API error and retries the request', () => {
    const error = new ApiError(500, 'Failed', 'Please try later.', {}, null);
    claimsApi.getClaims
      .mockReturnValueOnce(throwError(() => error))
      .mockReturnValueOnce(of([claim]));

    const fixture = TestBed.createComponent(ClaimListPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Please try later.');

    const retry = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    retry.click();
    fixture.detectChanges();

    expect(claimsApi.getClaims).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('MY-2026-001');
  });
});
