import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { ClaimDetail } from '../../../shared/models/claim.models';
import { AssessedLossForm } from '../assessed-loss-form/assessed-loss-form';

type OfficerClaimState =
  | { status: 'loading' }
  | { status: 'loaded'; claim: ClaimDetail }
  | { status: 'not-found' }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-officer-claim-page',
  imports: [
    AssessedLossForm,
    CurrencyPipe,
    DatePipe,
    LoadingIndicator,
    MatButtonModule,
    RouterLink,
    StatusBadge,
  ],
  templateUrl: './officer-claim-page.html',
  styleUrl: './officer-claim-page.scss',
})
export class OfficerClaimPage {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly claimId = inject(ActivatedRoute).snapshot.paramMap.get('id');
  protected readonly state = signal<OfficerClaimState>({ status: 'loading' });
  protected readonly successMessage = signal<string | null>(null);

  constructor() {
    this.loadClaim();
  }

  protected loadClaim(message?: string): void {
    if (!this.claimId) {
      this.state.set({ status: 'not-found' });
      return;
    }
    this.state.set({ status: 'loading' });
    this.successMessage.set(null);
    this.claimsApi
      .getClaim(this.claimId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (claim) => {
          this.state.set({ status: 'loaded', claim });
          this.successMessage.set(message ?? null);
        },
        error: (error: ApiError) =>
          this.state.set(
            error.status === 404 ? { status: 'not-found' } : { status: 'error', error },
          ),
      });
  }
}
