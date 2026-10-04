import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiError } from '../../../core/api/api-error';
import { OfficerWorkApiService } from '../../../core/api/officer-work-api.service';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { WorkClaimSummary } from '../../../shared/models/work-management.models';

type QueueState =
  | { status: 'loading' }
  | { status: 'loaded'; claims: WorkClaimSummary[] }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-officer-queue-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    EmptyState,
    LoadingIndicator,
    MatButtonModule,
    MatProgressSpinnerModule,
    StatusBadge,
  ],
  templateUrl: './officer-queue-page.html',
  styleUrl: './officer-queue-page.scss',
})
export class OfficerQueuePage {
  private readonly officerWorkApi = inject(OfficerWorkApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly state = signal<QueueState>({ status: 'loading' });
  protected readonly pickingClaimId = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly pickupError = signal<ApiError | null>(null);

  constructor() {
    this.loadQueue();
  }

  protected loadQueue(): void {
    this.state.set({ status: 'loading' });
    this.successMessage.set(null);
    this.pickupError.set(null);

    this.officerWorkApi
      .getQueue()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (claims) => this.state.set({ status: 'loaded', claims }),
        error: (error: ApiError) => this.state.set({ status: 'error', error }),
      });
  }

  protected assignToMe(claim: WorkClaimSummary): void {
    if (this.pickingClaimId() !== null) {
      return;
    }

    this.pickingClaimId.set(claim.id);
    this.successMessage.set(null);
    this.pickupError.set(null);

    this.officerWorkApi
      .assignToMe(claim.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (assignedClaim) => {
          this.pickingClaimId.set(null);
          this.state.update((currentState) =>
            currentState.status === 'loaded'
              ? {
                  status: 'loaded',
                  claims: currentState.claims.filter((item) => item.id !== assignedClaim.id),
                }
              : currentState,
          );
          this.successMessage.set(`${assignedClaim.referenceNumber} is now in your work.`);
        },
        error: (error: ApiError) => {
          this.pickingClaimId.set(null);
          this.pickupError.set(error);
        },
      });
  }
}
