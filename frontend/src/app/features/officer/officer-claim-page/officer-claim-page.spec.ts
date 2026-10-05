import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { ClaimWorkflowApiService } from '../../../core/api/claim-workflow-api.service';
import {
  ClaimDetail,
  ClaimStatus,
  ClaimType,
} from '../../../shared/models/claim.models';
import { AssessedLossForm } from '../assessed-loss-form/assessed-loss-form';
import { OfficerClaimPage } from './officer-claim-page';

const claim: ClaimDetail = {
  id: 'claim-1',
  referenceNumber: 'CLM-001',
  type: ClaimType.Motor,
  policyNumber: 'POL-001',
  market: 'MY',
  currency: 'MYR',
  incidentDate: '2026-10-01',
  incidentLocation: 'Kuala Lumpur',
  description: 'Rear bumper damage.',
  reportedLossAmount: 5000,
  assessedLossAmount: null,
  status: ClaimStatus.InReview,
  assignedOfficerId: 'officer-1',
  submittedAt: '2026-10-01T02:00:00Z',
  updatedAt: '2026-10-01T02:00:00Z',
  decisionReason: null,
  settlementAmount: null,
  informationRequests: [],
  history: [],
};

describe('OfficerClaimPage', () => {
  const claimsApi = { getClaim: vi.fn() };
  const workflowApi = {
    recordAssessedLoss: vi.fn(),
    requestInformation: vi.fn(),
    settle: vi.fn(),
    reject: vi.fn(),
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    claimsApi.getClaim.mockReturnValue(of(claim));
    workflowApi.recordAssessedLoss.mockReturnValue(
      of({ id: claim.id, status: claim.status, updatedAt: claim.updatedAt }),
    );

    await TestBed.configureTestingModule({
      imports: [OfficerClaimPage],
      providers: [
        { provide: ClaimsApiService, useValue: claimsApi },
        { provide: ClaimWorkflowApiService, useValue: workflowApi },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => claim.id } } },
        },
      ],
    }).compileComponents();
  });

  it('reflects a saved assessed loss in the financial summary', () => {
    const fixture = TestBed.createComponent(OfficerClaimPage);
    fixture.detectChanges();

    const assessment = fixture.debugElement.query(By.directive(AssessedLossForm))
      .componentInstance as AssessedLossForm;
    assessment['amount'].setValue(4200);
    assessment['submit']();
    fixture.detectChanges();

    expect(workflowApi.recordAssessedLoss).toHaveBeenCalledWith(claim.id, {
      amount: 4200,
    });
    expect(fixture.nativeElement.textContent).toMatch(/MYR\s*4,200\.00/);
    expect(fixture.nativeElement.textContent).toContain('Assessed loss saved.');
  });

  it('displays a date-only incident date without moving it to the previous day', () => {
    const fixture = TestBed.createComponent(OfficerClaimPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('1 Oct 2026');
    expect(fixture.nativeElement.textContent).not.toContain('30 Sep 2026');
  });
});
