import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import {
  ClaimDetail,
  ClaimHistoryEventType,
  ClaimStatus,
  ClaimType,
} from '../../../shared/models/claim.models';
import { ClaimDetailPage } from './claim-detail-page';

const claim: ClaimDetail = {
  id: 'claim-1',
  referenceNumber: 'MY-2026-001',
  type: ClaimType.Motor,
  policyNumber: 'POL-001',
  market: 'MY',
  currency: 'MYR',
  incidentDate: '2026-10-01',
  incidentLocation: 'Kuala Lumpur',
  description: 'Rear collision at traffic lights.',
  reportedLossAmount: 2500,
  assessedLossAmount: 1900,
  status: ClaimStatus.Settled,
  assignedOfficerId: 'officer-1',
  submittedAt: '2026-10-01T01:30:00Z',
  updatedAt: '2026-10-04T06:20:00Z',
  decisionReason: 'Covered repair cost',
  settlementAmount: 1700,
  informationRequests: [
    {
      id: 'request-1',
      requestedByOfficerId: 'officer-1',
      question: 'What is the police report reference?',
      requestedAt: '2026-10-03T04:00:00Z',
      response: 'Report 123',
      respondedAt: '2026-10-03T07:00:00Z',
    },
  ],
  history: [
    {
      id: 'history-1',
      actingUserId: 'claimant-1',
      eventType: ClaimHistoryEventType.Submitted,
      description: 'Claim submitted.',
      occurredAt: '2026-10-01T01:30:00Z',
    },
  ],
};

describe('ClaimDetailPage', () => {
  const claimsApi = {
    getClaim: vi.fn(),
  };

  beforeEach(async () => {
    claimsApi.getClaim.mockReset();

    await TestBed.configureTestingModule({
      imports: [ClaimDetailPage],
      providers: [
        provideRouter([]),
        { provide: ClaimsApiService, useValue: claimsApi },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: convertToParamMap({ id: 'claim-1' }) },
          },
        },
      ],
    }).compileComponents();
  });

  it('shows a loading state while the claim is requested', () => {
    claimsApi.getClaim.mockReturnValue(new Subject<ClaimDetail>());

    const fixture = TestBed.createComponent(ClaimDetailPage);
    fixture.detectChanges();

    expect(claimsApi.getClaim).toHaveBeenCalledWith('claim-1');
    expect(fixture.nativeElement.textContent).toContain('Loading claim details');
  });

  it('renders tracking, correspondence, history, and settlement details', () => {
    claimsApi.getClaim.mockReturnValue(of(claim));

    const fixture = TestBed.createComponent(ClaimDetailPage);
    fixture.detectChanges();
    const content = fixture.nativeElement.textContent;

    expect(content).toContain('MY-2026-001');
    expect(content).toContain('Kuala Lumpur');
    expect(content).toContain('What is the police report reference?');
    expect(content).toContain('Report 123');
    expect(content).toContain('Claim submitted.');
    expect(content).toContain('Claim settled');
    expect(content).toContain('Covered repair cost');
  });

  it('shows a not-found state for a missing or inaccessible claim', () => {
    claimsApi.getClaim.mockReturnValue(
      throwError(() => new ApiError(404, 'Not found', 'Missing', {}, null)),
    );

    const fixture = TestBed.createComponent(ClaimDetailPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Claim not found');
  });

  it('shows an error and retries the same claim', () => {
    const error = new ApiError(500, 'Failed', 'Please try later.', {}, null);
    claimsApi.getClaim.mockReturnValueOnce(throwError(() => error)).mockReturnValueOnce(of(claim));

    const fixture = TestBed.createComponent(ClaimDetailPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Please try later.');

    const retry = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    retry.click();
    fixture.detectChanges();

    expect(claimsApi.getClaim).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('MY-2026-001');
  });
});
