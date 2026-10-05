import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { ClaimDetail, ClaimHistoryEventType, ClaimStatus, ClaimType } from '../../../shared/models/claim.models';
import { ManagerClaimDetailPage } from './manager-claim-detail-page';

const claim: ClaimDetail = {
  id: 'claim-1', referenceNumber: 'CLM-001', type: ClaimType.Motor,
  policyNumber: 'POL-001', market: 'MY', currency: 'MYR', incidentDate: '2026-10-01',
  incidentLocation: 'Kuala Lumpur', description: 'Rear collision.', reportedLossAmount: 2500,
  assessedLossAmount: 1900, status: ClaimStatus.InReview, assignedOfficerId: 'officer-1',
  submittedAt: '2026-10-01T01:30:00Z', updatedAt: '2026-10-02T01:30:00Z',
  decisionReason: null, settlementAmount: null,
  informationRequests: [{ id: 'request-1', requestedByOfficerId: 'officer-1', question: 'Police report?', requestedAt: '2026-10-01T02:00:00Z', response: 'PR-123', respondedAt: '2026-10-01T03:00:00Z' }],
  history: [{ id: 'history-1', actingUserId: 'claimant-1', eventType: ClaimHistoryEventType.Submitted, description: 'Claim submitted.', occurredAt: '2026-10-01T01:30:00Z' }],
};

describe('ManagerClaimDetailPage', () => {
  const claimsApi = { getClaim: vi.fn() };

  beforeEach(async () => {
    claimsApi.getClaim.mockReset();
    await TestBed.configureTestingModule({
      imports: [ManagerClaimDetailPage],
      providers: [
        provideRouter([]),
        { provide: ClaimsApiService, useValue: claimsApi },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: 'claim-1' }) } } },
      ],
    }).compileComponents();
  });

  it('shows the scoped claim as read-only detail', () => {
    claimsApi.getClaim.mockReturnValue(of(claim));
    const fixture = TestBed.createComponent(ManagerClaimDetailPage);
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(claimsApi.getClaim).toHaveBeenCalledWith('claim-1');
    expect(content).toContain('CLM-001');
    expect(content).toContain('Police report?');
    expect(content).toContain('Claim submitted.');
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });
});
