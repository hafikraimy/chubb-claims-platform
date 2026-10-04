import { Component, inject, signal } from '@angular/core';
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
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { ClaimType, SubmitClaimRequest } from '../../../shared/models/claim.models';

interface SubmitClaimForm {
  type: FormControl<ClaimType | null>;
  policyNumber: FormControl<string>;
  currency: FormControl<string>;
  incidentDate: FormControl<string>;
  incidentLocation: FormControl<string>;
  description: FormControl<string>;
  reportedLossAmount: FormControl<number | null>;
}

@Component({
  selector: 'app-submit-claim-page',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './submit-claim-page.html',
  styleUrl: './submit-claim-page.scss',
})
export class SubmitClaimPage {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly router = inject(Router);

  protected readonly ClaimType = ClaimType;
  protected readonly submitting = signal(false);
  protected readonly submissionError = signal<string | null>(null);
  protected readonly maxIncidentDate = this.todayAsIsoDate();
  protected readonly form = new FormGroup<SubmitClaimForm>({
    type: new FormControl<ClaimType | null>(null, Validators.required),
    policyNumber: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.nonWhitespace(), Validators.maxLength(50)],
    }),
    currency: new FormControl('MYR', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^[A-Za-z]{3}$/)],
    }),
    incidentDate: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.notAfter(this.maxIncidentDate)],
    }),
    incidentLocation: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.nonWhitespace(), Validators.maxLength(200)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.nonWhitespace(), Validators.maxLength(2000)],
    }),
    reportedLossAmount: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
    ]),
  });

  protected submit(): void {
    this.submissionError.set(null);
    this.clearServerErrors();
    this.form.markAllAsTouched();

    if (this.form.invalid || this.submitting()) {
      return;
    }

    const value = this.form.getRawValue();
    if (value.type === null || value.reportedLossAmount === null) {
      return;
    }

    const request: SubmitClaimRequest = {
      type: value.type,
      policyNumber: value.policyNumber.trim(),
      currency: value.currency.trim().toUpperCase(),
      incidentDate: value.incidentDate,
      incidentLocation: value.incidentLocation.trim(),
      description: value.description.trim(),
      reportedLossAmount: value.reportedLossAmount,
    };

    this.submitting.set(true);
    this.form.disable();

    this.claimsApi.submitClaim(request).subscribe({
      next: (created) => {
        void this.router.navigate(['/claimant/claims', created.id]);
      },
      error: (error: ApiError) => {
        this.submitting.set(false);
        this.form.enable();
        this.applyServerErrors(error.validationErrors);
        this.submissionError.set(error.detail);
      },
    });
  }

  protected serverError(control: AbstractControl): string | null {
    const errors = control.getError('server') as readonly string[] | undefined;
    return errors?.[0] ?? null;
  }

  private applyServerErrors(errors: Readonly<Record<string, readonly string[]>>): void {
    const controls: Record<string, AbstractControl> = {
      type: this.form.controls.type,
      policyNumber: this.form.controls.policyNumber,
      currency: this.form.controls.currency,
      incidentDate: this.form.controls.incidentDate,
      incidentLocation: this.form.controls.incidentLocation,
      description: this.form.controls.description,
      reportedLossAmount: this.form.controls.reportedLossAmount,
    };

    for (const [field, messages] of Object.entries(errors)) {
      const key = Object.keys(controls).find(
        (controlName) => controlName.toLowerCase() === field.toLowerCase(),
      );

      if (key) {
        const control = controls[key];
        control.setErrors({ ...control.errors, server: messages });
      }
    }
  }

  private clearServerErrors(): void {
    for (const control of Object.values(this.form.controls)) {
      if (!control.errors?.['server']) {
        continue;
      }

      const { server: _, ...remainingErrors } = control.errors;
      control.setErrors(Object.keys(remainingErrors).length > 0 ? remainingErrors : null);
    }
  }

  private todayAsIsoDate(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private nonWhitespace(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null =>
      typeof control.value === 'string' && control.value.trim().length === 0
        ? { whitespace: true }
        : null;
  }

  private notAfter(maximumDate: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null =>
      typeof control.value === 'string' && control.value > maximumDate
        ? { futureDate: true }
        : null;
  }
}
