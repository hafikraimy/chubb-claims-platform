import { HttpErrorResponse } from '@angular/common/http';
import { Service } from '@angular/core';
import { ApiError, ApiProblemDetails } from './api-error';

@Service()
export class ApiErrorService {
  normalize(error: unknown): ApiError {
    if (!(error instanceof HttpErrorResponse)) {
      return new ApiError(
        0,
        'Unexpected error',
        'Something went wrong. Please try again.',
        {},
        error,
      );
    }

    if (error.status === 0) {
      return new ApiError(
        0,
        'API unavailable',
        'Unable to reach the claims API. Check that it is running.',
        {},
        error,
      );
    }

    const problem = this.readProblemDetails(error.error);

    return new ApiError(
      error.status,
      problem?.title ?? this.defaultTitle(error.status),
      problem?.detail ?? this.defaultDetail(error.status),
      problem?.errors ?? {},
      error,
    );
  }

  private readProblemDetails(value: unknown): ApiProblemDetails | null {
    if (!this.isRecord(value)) {
      return null;
    }

    return {
      type: this.readString(value, 'type'),
      title: this.readString(value, 'title'),
      status: this.readNumber(value, 'status'),
      detail: this.readString(value, 'detail'),
      instance: this.readString(value, 'instance'),
      errors: this.readValidationErrors(value['errors']),
      traceId: this.readString(value, 'traceId'),
    };
  }

  private readValidationErrors(value: unknown): Record<string, string[]> | undefined {
    if (!this.isRecord(value)) {
      return undefined;
    }

    const entries = Object.entries(value).filter(
      (entry): entry is [string, string[]] =>
        Array.isArray(entry[1]) && entry[1].every((message) => typeof message === 'string'),
    );

    return entries.length > 0 ? Object.fromEntries(entries) : undefined;
  }

  private readString(value: Record<string, unknown>, property: string): string | undefined {
    const candidate = value[property];
    return typeof candidate === 'string' ? candidate : undefined;
  }

  private readNumber(value: Record<string, unknown>, property: string): number | undefined {
    const candidate = value[property];
    return typeof candidate === 'number' ? candidate : undefined;
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null && !Array.isArray(value);
  }

  private defaultTitle(status: number): string {
    switch (status) {
      case 400:
        return 'Validation failed';
      case 401:
        return 'Session required';
      case 403:
        return 'Access denied';
      case 404:
        return 'Not found';
      case 409:
        return 'Operation not allowed';
      default:
        return status >= 500 ? 'Unexpected server error' : 'Request failed';
    }
  }

  private defaultDetail(status: number): string {
    switch (status) {
      case 400:
        return 'Check the information provided and try again.';
      case 401:
        return 'Choose a demo user to continue.';
      case 403:
        return 'You do not have access to perform this action.';
      case 404:
        return 'The requested resource could not be found.';
      case 409:
        return 'The request conflicts with the current claim state.';
      default:
        return status >= 500
          ? 'The claims API could not complete the request.'
          : 'The request could not be completed.';
    }
  }
}
