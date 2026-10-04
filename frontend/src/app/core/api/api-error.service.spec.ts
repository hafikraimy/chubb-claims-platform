import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorService } from './api-error.service';

describe('ApiErrorService', () => {
  const service = new ApiErrorService();

  it('normalizes RFC 7807 details and validation errors', () => {
    const original = new HttpErrorResponse({
      status: 400,
      error: {
        title: 'Validation failed',
        status: 400,
        detail: 'One or more values are invalid.',
        errors: {
          policyNumber: ['The policy number is required.'],
        },
      },
    });

    const error = service.normalize(original);

    expect(error.status).toBe(400);
    expect(error.title).toBe('Validation failed');
    expect(error.detail).toBe('One or more values are invalid.');
    expect(error.validationErrors['policyNumber']).toEqual(['The policy number is required.']);
    expect(error.isValidationError).toBe(true);
  });

  it('provides a useful network failure message', () => {
    const error = service.normalize(new HttpErrorResponse({ status: 0 }));

    expect(error.title).toBe('API unavailable');
    expect(error.detail).toContain('Check that it is running');
  });

  it('provides a fallback for an empty conflict response', () => {
    const error = service.normalize(new HttpErrorResponse({ status: 409 }));

    expect(error.title).toBe('Operation not allowed');
    expect(error.detail).toContain('current claim state');
  });

  it('handles an unknown thrown value without an unsafe cast', () => {
    const original = new Error('unexpected');
    const error = service.normalize(original);

    expect(error.status).toBe(0);
    expect(error.originalError).toBe(original);
  });
});
