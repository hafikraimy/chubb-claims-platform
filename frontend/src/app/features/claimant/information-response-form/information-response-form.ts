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
import { ClaimsApiService } from '../../../core/api/claims-api.service';

@Component({
  selector: 'app-information-response-form',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
  ],
  templateUrl: './information-response-form.html',
  styleUrl: './information-response-form.scss',
})
export class InformationResponseForm {
  private readonly claimsApi = inject(ClaimsApiService);

  readonly claimId = input.required<string>();
  readonly requestId = input.required<string>();
  readonly submitted = output<void>();
  readonly refreshRequested = output<void>();

  protected readonly response = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, this.nonWhitespace(), Validators.maxLength(2000)],
  });
  protected readonly form = new FormGroup({ response: this.response });
  protected readonly submitting = signal(false);
  protected readonly submissionError = signal<string | null>(null);
  protected readonly conflict = signal(false);

  protected submit(): void {
    this.submissionError.set(null);
    this.conflict.set(false);
    this.clearServerError();
    this.response.markAsTouched();

    if (this.response.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.response.disable();

    this.claimsApi
      .respondToInformationRequest(this.claimId(), this.requestId(), {
        response: this.response.getRawValue().trim(),
      })
      .subscribe({
        next: () => this.submitted.emit(),
        error: (error: ApiError) => {
          this.submitting.set(false);
          this.response.enable();
          this.conflict.set(error.status === 409);
          this.applyServerError(error.validationErrors);
          this.submissionError.set(error.detail);
        },
      });
  }

  protected serverError(): string | null {
    const messages = this.response.getError('server') as readonly string[] | undefined;
    return messages?.[0] ?? null;
  }

  private applyServerError(errors: Readonly<Record<string, readonly string[]>>): void {
    const responseEntry = Object.entries(errors).find(
      ([field]) => field.toLowerCase() === 'response',
    );

    if (responseEntry) {
      this.response.setErrors({
        ...this.response.errors,
        server: responseEntry[1],
      });
    }
  }

  private clearServerError(): void {
    if (!this.response.errors?.['server']) {
      return;
    }

    const { server: _, ...remainingErrors } = this.response.errors;
    this.response.setErrors(Object.keys(remainingErrors).length > 0 ? remainingErrors : null);
  }

  private nonWhitespace(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null =>
      typeof control.value === 'string' && control.value.trim().length === 0
        ? { whitespace: true }
        : null;
  }
}
