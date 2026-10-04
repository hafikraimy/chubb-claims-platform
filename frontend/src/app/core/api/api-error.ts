export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly title: string,
    readonly detail: string,
    readonly validationErrors: Readonly<Record<string, readonly string[]>>,
    readonly originalError: unknown,
  ) {
    super(detail);
    this.name = 'ApiError';
  }

  get isValidationError(): boolean {
    return Object.keys(this.validationErrors).length > 0;
  }
}
