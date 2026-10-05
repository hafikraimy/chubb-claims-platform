import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ClaimWorkflowApiService } from '../../../core/api/claim-workflow-api.service';
import { ClaimStatus } from '../../../shared/models/claim.models';
import { OfficerWorkflowActions } from './officer-workflow-actions';

describe('OfficerWorkflowActions', () => {
  const workflowApi = { requestInformation: vi.fn(), settle: vi.fn(), reject: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [OfficerWorkflowActions],
      providers: [{ provide: ClaimWorkflowApiService, useValue: workflowApi }],
    }).compileComponents();
  });

  function createFixture() {
    const fixture = TestBed.createComponent(OfficerWorkflowActions);
    fixture.componentRef.setInput('claimId', 'claim-1');
    fixture.componentRef.setInput('currency', 'MYR');
    fixture.detectChanges();
    return fixture;
  }

  it('sends an information request', () => {
    workflowApi.requestInformation.mockReturnValue(
      of({
        id: 'request-1',
        claimStatus: ClaimStatus.AwaitingInfo,
        requestedAt: '2026-10-05T00:00:00Z',
      }),
    );
    const fixture = createFixture();
    const completed = vi.fn();
    fixture.componentInstance.completed.subscribe(completed);
    fixture.componentInstance['informationForm'].controls.question.setValue(
      'Please provide the report.',
    );
    fixture.componentInstance['requestInformation']();
    expect(workflowApi.requestInformation).toHaveBeenCalledWith('claim-1', {
      question: 'Please provide the report.',
    });
    expect(completed).toHaveBeenCalledWith('Information request sent.');
  });

  it('settles a claim with an amount and reason', () => {
    workflowApi.settle.mockReturnValue(
      of({ id: 'claim-1', status: ClaimStatus.Settled, updatedAt: '2026-10-05T00:00:00Z' }),
    );
    const fixture = createFixture();
    fixture.componentInstance['settleForm'].setValue({
      settlementAmount: 1800,
      reason: 'Covered loss.',
    });
    fixture.componentInstance['settle']();
    expect(workflowApi.settle).toHaveBeenCalledWith('claim-1', {
      settlementAmount: 1800,
      reason: 'Covered loss.',
    });
  });

  it('rejects a claim and exposes workflow conflicts', () => {
    workflowApi.reject.mockReturnValue(
      throwError(() => new ApiError(409, 'Conflict', 'Claim changed.', {}, null)),
    );
    const fixture = createFixture();
    fixture.componentInstance['rejectForm'].setValue({ reason: 'Not covered.' });
    fixture.componentInstance['reject']();
    fixture.detectChanges();
    expect(workflowApi.reject).toHaveBeenCalledWith('claim-1', { reason: 'Not covered.' });
    expect(fixture.nativeElement.textContent).toContain('Refresh claim');
  });
});
