import { TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { provideRouter, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import {
  ClaimCreated,
  ClaimStatus,
  ClaimType,
  SubmitClaimRequest,
} from '../../../shared/models/claim.models';
import { SubmitClaimPage } from './submit-claim-page';

const validFormValue = {
  type: ClaimType.Motor,
  policyNumber: '  POL-001  ',
  currency: 'myr',
  incidentDate: '2026-10-01',
  incidentLocation: '  Kuala Lumpur  ',
  description: '  Rear collision at traffic lights.  ',
  reportedLossAmount: 2500,
};

const created: ClaimCreated = {
  id: 'claim-1',
  referenceNumber: 'CLM-001',
  status: ClaimStatus.Submitted,
};

describe('SubmitClaimPage', () => {
  const claimsApi = {
    submitClaim: vi.fn(),
  };
  let router: Router;

  beforeEach(async () => {
    claimsApi.submitClaim.mockReset();

    await TestBed.configureTestingModule({
      imports: [SubmitClaimPage],
      providers: [provideRouter([]), { provide: ClaimsApiService, useValue: claimsApi }],
    }).compileComponents();

    router = TestBed.inject(Router);
  });

  it('does not submit an invalid form and displays validation errors', () => {
    const fixture = TestBed.createComponent(SubmitClaimPage);
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    expect(claimsApi.submitClaim).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Select a claim type');
    expect(fixture.nativeElement.textContent).toContain('Enter the policy number');
  });

  it('normalizes the request and navigates to the created claim', () => {
    claimsApi.submitClaim.mockReturnValue(of(created));
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(SubmitClaimPage);
    const form = getForm(fixture.componentInstance);
    form.setValue(validFormValue);

    fixture.detectChanges();
    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();

    expect(claimsApi.submitClaim).toHaveBeenCalledWith({
      type: ClaimType.Motor,
      policyNumber: 'POL-001',
      currency: 'MYR',
      incidentDate: '2026-10-01',
      incidentLocation: 'Kuala Lumpur',
      description: 'Rear collision at traffic lights.',
      reportedLossAmount: 2500,
    } satisfies SubmitClaimRequest);
    expect(navigate).toHaveBeenCalledWith(['/claimant/claims', 'claim-1']);
  });

  it('disables the form while submission is in progress', () => {
    claimsApi.submitClaim.mockReturnValue(new Subject<ClaimCreated>());
    const fixture = TestBed.createComponent(SubmitClaimPage);
    const form = getForm(fixture.componentInstance);
    form.setValue(validFormValue);
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    expect(form.disabled).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Submitting');
  });

  it('maps API field errors back to the matching control', () => {
    claimsApi.submitClaim.mockReturnValue(
      throwError(
        () =>
          new ApiError(
            400,
            'Validation failed',
            'Check the submitted values.',
            { PolicyNumber: ['The policy number is invalid.'] },
            null,
          ),
      ),
    );
    const fixture = TestBed.createComponent(SubmitClaimPage);
    const form = getForm(fixture.componentInstance);
    form.setValue(validFormValue);
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    expect(form.enabled).toBe(true);
    expect(form.controls['policyNumber'].getError('server')).toEqual([
      'The policy number is invalid.',
    ]);
    expect(fixture.nativeElement.textContent).toContain('The policy number is invalid.');
    expect(fixture.nativeElement.textContent).toContain('Check the submitted values.');
  });

  it('rejects a future incident date', () => {
    const fixture = TestBed.createComponent(SubmitClaimPage);
    const form = getForm(fixture.componentInstance);
    form.patchValue({ incidentDate: '9999-12-31' });
    form.controls['incidentDate'].markAsTouched();
    fixture.detectChanges();

    expect(form.controls['incidentDate'].hasError('futureDate')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain(
      'The incident date cannot be in the future.',
    );
  });

  function getForm(component: SubmitClaimPage): FormGroup {
    return (component as unknown as { form: FormGroup }).form;
  }
});
