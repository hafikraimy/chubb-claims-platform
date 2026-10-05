import { Component, inject, input, output, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiError } from '../../../core/api/api-error';
import { ClaimWorkflowApiService } from '../../../core/api/claim-workflow-api.service';
import { Observable } from 'rxjs';

type ActionType = 'information' | 'settle' | 'reject';

@Component({
  selector: 'app-officer-workflow-actions',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
  ],
  templateUrl: './officer-workflow-actions.html',
  styleUrl: './officer-workflow-actions.scss',
})
export class OfficerWorkflowActions {
  private readonly workflowApi = inject(ClaimWorkflowApiService);
  readonly claimId = input.required<string>();
  readonly currency = input.required<string>();
  readonly completed = output<string>();
  readonly refreshRequested = output<void>();
  protected readonly activeAction = signal<ActionType | null>(null);
  protected readonly submitting = signal(false);
  protected readonly submissionError = signal<string | null>(null);
  protected readonly conflict = signal(false);
  protected readonly informationForm = new FormGroup({ question: this.textControl(1000) });
  protected readonly settleForm = new FormGroup({
    settlementAmount: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
    ]),
    reason: this.textControl(2000),
  });
  protected readonly rejectForm = new FormGroup({ reason: this.textControl(2000) });

  protected selectAction(action: ActionType): void {
    this.activeAction.set(this.activeAction() === action ? null : action);
    this.submissionError.set(null);
    this.conflict.set(false);
  }

  protected requestInformation(): void {
    this.informationForm.markAllAsTouched();
    if (this.informationForm.invalid || this.submitting()) return;
    this.startRequest(
      this.workflowApi.requestInformation(this.claimId(), {
        question: this.informationForm.getRawValue().question.trim(),
      }),
      'Information request sent.',
    );
  }

  protected settle(): void {
    this.settleForm.markAllAsTouched();
    if (this.settleForm.invalid || this.submitting()) return;
    const value = this.settleForm.getRawValue();
    if (value.settlementAmount === null) return;
    this.startRequest(
      this.workflowApi.settle(this.claimId(), {
        settlementAmount: value.settlementAmount,
        reason: value.reason.trim(),
      }),
      'Claim settled.',
    );
  }

  protected reject(): void {
    this.rejectForm.markAllAsTouched();
    if (this.rejectForm.invalid || this.submitting()) return;
    this.startRequest(
      this.workflowApi.reject(this.claimId(), {
        reason: this.rejectForm.getRawValue().reason.trim(),
      }),
      'Claim rejected.',
    );
  }

  private startRequest(request: Observable<unknown>, message: string): void {
    this.submitting.set(true);
    this.submissionError.set(null);
    this.conflict.set(false);
    request.subscribe({
      next: () => this.completed.emit(message),
      error: (error: ApiError) => {
        this.submitting.set(false);
        this.conflict.set(error.status === 409);
        this.submissionError.set(error.detail);
      },
    });
  }

  private textControl(maxLength: number): FormControl<string> {
    return new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.nonWhitespace(), Validators.maxLength(maxLength)],
    });
  }

  private nonWhitespace(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null =>
      typeof control.value === 'string' && control.value.trim().length === 0
        ? { whitespace: true }
        : null;
  }
}
