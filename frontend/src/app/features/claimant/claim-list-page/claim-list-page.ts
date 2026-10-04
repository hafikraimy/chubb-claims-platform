import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { ClaimSummary } from '../../../shared/models/claim.models';

type ClaimListState =
  | { status: 'loading' }
  | { status: 'loaded'; claims: ClaimSummary[] }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-claim-list-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    EmptyState,
    LoadingIndicator,
    MatButtonModule,
    RouterLink,
    StatusBadge,
  ],
  templateUrl: './claim-list-page.html',
  styleUrl: './claim-list-page.scss',
})
export class ClaimListPage {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly state = signal<ClaimListState>({ status: 'loading' });

  constructor() {
    this.loadClaims();
  }

  protected loadClaims(): void {
    this.state.set({ status: 'loading' });

    this.claimsApi
      .getClaims()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (claims) => this.state.set({ status: 'loaded', claims }),
        error: (error: ApiError) => this.state.set({ status: 'error', error }),
      });
  }
}
