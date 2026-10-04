import { TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { ClaimStatus, ClaimUpdated } from '../../../shared/models/claim.models';
import { InformationResponseForm } from './information-response-form';

const updated: ClaimUpdated = {
  id: 'claim-1',
  status: ClaimStatus.InReview,
  updatedAt: '2026-10-05T01:00:00Z',
};

describe('InformationResponseForm', () => {
  const claimsApi = {
    respondToInformationRequest: vi.fn(),
  };

  beforeEach(async () => {
    claimsApi.respondToInformationRequest.mockReset();

    await TestBed.configureTestingModule({
      imports: [InformationResponseForm],
      providers: [{ provide: ClaimsApiService, useValue: claimsApi }],
    }).compileComponents();
  });

  it('blocks an empty response', () => {
    const fixture = createFixture();
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    expect(claimsApi.respondToInformationRequest).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Enter a response before submitting');
  });

  it('trims and submits the response', () => {
    claimsApi.respondToInformationRequest.mockReturnValue(of(updated));
    const fixture = createFixture();
    const submitted = vi.fn();
    fixture.componentInstance.submitted.subscribe(submitted);
    getResponse(fixture.componentInstance).setValue('  Police report 123  ');
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();

    expect(claimsApi.respondToInformationRequest).toHaveBeenCalledWith('claim-1', 'request-1', {
      response: 'Police report 123',
    });
    expect(submitted).toHaveBeenCalledOnce();
  });

  it('locks the response while it is being sent', () => {
    claimsApi.respondToInformationRequest.mockReturnValue(new Subject<ClaimUpdated>());
    const fixture = createFixture();
    const response = getResponse(fixture.componentInstance);
    response.setValue('Police report 123');
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    expect(response.disabled).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Sending');
  });

  it('maps server validation to the response field', () => {
    claimsApi.respondToInformationRequest.mockReturnValue(
      throwError(
        () =>
          new ApiError(
            400,
            'Validation failed',
            'Check the response.',
            { Response: ['A more detailed response is required.'] },
            null,
          ),
      ),
    );
    const fixture = createFixture();
    const response = getResponse(fixture.componentInstance);
    response.setValue('Report');
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    expect(response.enabled).toBe(true);
    expect(response.getError('server')).toEqual(['A more detailed response is required.']);
    expect(fixture.nativeElement.textContent).toContain('A more detailed response is required.');
  });

  it('offers a claim reload after a workflow conflict', () => {
    claimsApi.respondToInformationRequest.mockReturnValue(
      throwError(
        () =>
          new ApiError(
            409,
            'Operation not allowed',
            'This information request has already been answered.',
            {},
            null,
          ),
      ),
    );
    const fixture = createFixture();
    const refreshRequested = vi.fn();
    fixture.componentInstance.refreshRequested.subscribe(refreshRequested);
    getResponse(fixture.componentInstance).setValue('Report 123');
    fixture.detectChanges();

    const submit = fixture.nativeElement.querySelector(
      'button[type="submit"]',
    ) as HTMLButtonElement;
    submit.click();
    fixture.detectChanges();

    const reload = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((button) => button.textContent?.includes('Reload claim'));
    reload?.click();

    expect(refreshRequested).toHaveBeenCalledOnce();
  });

  function createFixture() {
    const fixture = TestBed.createComponent(InformationResponseForm);
    fixture.componentRef.setInput('claimId', 'claim-1');
    fixture.componentRef.setInput('requestId', 'request-1');
    return fixture;
  }

  function getResponse(component: InformationResponseForm): FormControl<string> {
    return (component as unknown as { response: FormControl<string> }).response;
  }
});
