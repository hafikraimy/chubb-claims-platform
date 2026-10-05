import { TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ClaimWorkflowApiService } from '../../../core/api/claim-workflow-api.service';
import { ClaimStatus, ClaimUpdated } from '../../../shared/models/claim.models';
import { AssessedLossForm } from './assessed-loss-form';

describe('AssessedLossForm', () => {
  const workflowApi = { recordAssessedLoss: vi.fn() };

  beforeEach(async () => {
    workflowApi.recordAssessedLoss.mockReset();
    await TestBed.configureTestingModule({
      imports: [AssessedLossForm],
      providers: [{ provide: ClaimWorkflowApiService, useValue: workflowApi }],
    }).compileComponents();
  });

  function createFixture() {
    const fixture = TestBed.createComponent(AssessedLossForm);
    fixture.componentRef.setInput('claimId', 'claim-1');
    fixture.componentRef.setInput('currency', 'MYR');
    fixture.componentRef.setInput('currentAmount', null);
    fixture.detectChanges();
    return fixture;
  }

  it('requires a positive amount', () => {
    const fixture = createFixture();
    fixture.componentInstance['submit']();
    fixture.detectChanges();
    expect(workflowApi.recordAssessedLoss).not.toHaveBeenCalled();
  });

  it('records an assessed loss and emits saved', () => {
    const updated: ClaimUpdated = {
      id: 'claim-1',
      status: ClaimStatus.InReview,
      updatedAt: '2026-10-05T00:00:00Z',
    };
    workflowApi.recordAssessedLoss.mockReturnValue(of(updated));
    const fixture = createFixture();
    const saved = vi.fn();
    fixture.componentInstance.saved.subscribe(saved);
    fixture.componentInstance['amount'].setValue(2200);
    fixture.componentInstance['submit']();
    expect(workflowApi.recordAssessedLoss).toHaveBeenCalledWith('claim-1', { amount: 2200 });
    expect(saved).toHaveBeenCalled();
  });

  it('locks the form while saving and offers refresh on conflict', () => {
    const pending = new Subject<ClaimUpdated>();
    workflowApi.recordAssessedLoss.mockReturnValue(pending);
    const fixture = createFixture();
    fixture.componentInstance['amount'].setValue(2200);
    fixture.componentInstance['submit']();
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.disabled).toBe(true);

    pending.error(new ApiError(409, 'Conflict', 'Claim changed.', {}, null));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Refresh claim');
  });
});
