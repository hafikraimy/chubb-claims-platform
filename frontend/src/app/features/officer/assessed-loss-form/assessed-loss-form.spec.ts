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

  function createFixture(currentAmount: number | null = null) {
    const fixture = TestBed.createComponent(AssessedLossForm);
    fixture.componentRef.setInput('claimId', 'claim-1');
    fixture.componentRef.setInput('currency', 'MYR');
    fixture.componentRef.setInput('currentAmount', currentAmount);
    fixture.detectChanges();
    return fixture;
  }

  it('requires a positive amount', () => {
    const fixture = createFixture();
    fixture.componentInstance['submit']();
    fixture.detectChanges();
    expect(workflowApi.recordAssessedLoss).not.toHaveBeenCalled();
  });

  it('shows the current assessed loss so it can be updated', () => {
    const fixture = createFixture(2200);

    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;

    expect(input.valueAsNumber).toBe(2200);
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
    expect(saved).toHaveBeenCalledWith(2200);
  });

  it('saves from an explicit button click without native form submission', () => {
    workflowApi.recordAssessedLoss.mockReturnValue(
      of({
        id: 'claim-1',
        status: ClaimStatus.InReview,
        updatedAt: '2026-10-05T00:00:00Z',
      }),
    );
    const fixture = createFixture();
    fixture.componentInstance['amount'].setValue(2200);

    const button = fixture.nativeElement.querySelector(
      '.form-row button',
    ) as HTMLButtonElement;
    expect(button.type).toBe('button');
    button.click();

    expect(workflowApi.recordAssessedLoss).toHaveBeenCalledWith('claim-1', {
      amount: 2200,
    });
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
