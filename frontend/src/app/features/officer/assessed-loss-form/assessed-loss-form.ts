import { Component, inject, input, output, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiError } from '../../../core/api/api-error';
import { ClaimWorkflowApiService } from '../../../core/api/claim-workflow-api.service';

@Component({
  selector: 'app-assessed-loss-form',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
  ],
  templateUrl: './assessed-loss-form.html',
  styleUrl: './assessed-loss-form.scss',
})
export class AssessedLossForm {
  private readonly workflowApi = inject(ClaimWorkflowApiService);
  readonly claimId = input.required<string>();
  readonly currency = input.required<string>();
  readonly currentAmount = input.required<number | null>();
  readonly saved = output<void>();
  readonly refreshRequested = output<void>();
  protected readonly amount = new FormControl<number | null>(null, [
    Validators.required,
    Validators.min(0.01),
  ]);
  protected readonly submitting = signal(false);
  protected readonly submissionError = signal<string | null>(null);
  protected readonly conflict = signal(false);

  protected submit(): void {
    this.amount.markAsTouched();
    this.submissionError.set(null);
    this.conflict.set(false);
    if (this.amount.invalid || this.amount.value === null || this.submitting()) return;

    const amount = this.amount.value;
    this.submitting.set(true);
    this.amount.disable();
    this.workflowApi.recordAssessedLoss(this.claimId(), { amount }).subscribe({
      next: () => this.saved.emit(),
      error: (error: ApiError) => {
        this.submitting.set(false);
        this.amount.enable();
        this.conflict.set(error.status === 409);
        this.submissionError.set(error.detail);
      },
    });
  }
}
