import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ClaimType,
  CreateInformationRequest,
  RecordAssessedLoss,
  RejectClaim,
  RespondToInformationRequest,
  SettleClaim,
  SubmitClaimRequest,
} from '../../shared/models/claim.models';
import { AssignClaimRequest } from '../../shared/models/work-management.models';
import { ClaimWorkflowApiService } from './claim-workflow-api.service';
import { ClaimsApiService } from './claims-api.service';
import { ManagerApiService } from './manager-api.service';
import { OfficerWorkApiService } from './officer-work-api.service';

describe('API services', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses the claims endpoints and request bodies', () => {
    const service = TestBed.inject(ClaimsApiService);
    const submission: SubmitClaimRequest = {
      type: ClaimType.Motor,
      policyNumber: 'POL-001',
      currency: 'MYR',
      incidentDate: '2026-10-01',
      incidentLocation: 'Kuala Lumpur',
      description: 'Rear collision',
      reportedLossAmount: 2500,
    };
    const response: RespondToInformationRequest = { response: 'Report 123' };

    service.getClaims().subscribe();
    expectRequest('/api/claims', 'GET');

    service.getClaim('claim-1').subscribe();
    expectRequest('/api/claims/claim-1', 'GET');

    service.submitClaim(submission).subscribe();
    expectRequest('/api/claims', 'POST', submission);

    service.respondToInformationRequest('claim-1', 'request-1', response).subscribe();
    expectRequest('/api/claims/claim-1/information-requests/request-1/response', 'POST', response);
  });

  it('uses the officer workflow endpoints and request bodies', () => {
    const service = TestBed.inject(ClaimWorkflowApiService);
    const assessment: RecordAssessedLoss = { amount: 1800 };
    const informationRequest: CreateInformationRequest = {
      question: 'What is the police report reference?',
    };
    const settlement: SettleClaim = {
      settlementAmount: 1700,
      reason: 'Covered repair cost',
    };
    const rejection: RejectClaim = { reason: 'Not covered' };

    service.recordAssessedLoss('claim-1', assessment).subscribe();
    expectRequest('/api/claims/claim-1/assessed-loss', 'POST', assessment);

    service.requestInformation('claim-1', informationRequest).subscribe();
    expectRequest('/api/claims/claim-1/information-requests', 'POST', informationRequest);

    service.settle('claim-1', settlement).subscribe();
    expectRequest('/api/claims/claim-1/settle', 'POST', settlement);

    service.reject('claim-1', rejection).subscribe();
    expectRequest('/api/claims/claim-1/reject', 'POST', rejection);
  });

  it('uses the officer work endpoints', () => {
    const service = TestBed.inject(OfficerWorkApiService);

    service.getQueue().subscribe();
    expectRequest('/api/work/queue', 'GET');

    service.getMyClaims().subscribe();
    expectRequest('/api/work/my-claims', 'GET');

    service.getTeamSummary().subscribe();
    expectRequest('/api/work/team-summary', 'GET');

    service.assignToMe('claim-1').subscribe();
    expectRequest('/api/claims/claim-1/assign-to-me', 'POST', {});
  });

  it('uses the manager endpoints and assignment body', () => {
    const service = TestBed.inject(ManagerApiService);
    const assignment: AssignClaimRequest = { officerId: 'officer-1' };

    service.getDashboard().subscribe();
    expectRequest('/api/manager/dashboard', 'GET');

    service.assignClaim('claim-1', assignment).subscribe();
    expectRequest('/api/claims/claim-1/assignments', 'POST', assignment);
  });

  function expectRequest(url: string, method: string, body?: unknown): void {
    const request = http.expectOne(url);
    expect(request.request.method).toBe(method);

    if (body !== undefined) {
      expect(request.request.body).toEqual(body);
    }

    request.flush({});
  }
});
