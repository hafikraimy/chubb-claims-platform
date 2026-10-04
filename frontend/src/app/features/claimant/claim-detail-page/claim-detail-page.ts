import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError } from '../../../core/api/api-error';
import { ClaimsApiService } from '../../../core/api/claims-api.service';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { ClaimDetail, ClaimStatus } from '../../../shared/models/claim.models';
import { InformationResponseForm } from '../information-response-form/information-response-form';

type ClaimDetailState =
  | { status: 'loading' }
  | { status: 'loaded'; claim: ClaimDetail }
  | { status: 'not-found' }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-claim-detail-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    InformationResponseForm,
    LoadingIndicator,
    MatButtonModule,
    RouterLink,
    StatusBadge,
  ],
  templateUrl: './claim-detail-page.html',
  styleUrl: './claim-detail-page.scss',
})
export class ClaimDetailPage {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly claimId = this.route.snapshot.paramMap.get('id');

  protected readonly ClaimStatus = ClaimStatus;
  protected readonly state = signal<ClaimDetailState>({ status: 'loading' });

  constructor() {
    this.loadClaim();
  }

  protected loadClaim(): void {
    if (!this.claimId) {
      this.state.set({ status: 'not-found' });
      return;
    }

    this.state.set({ status: 'loading' });
    this.claimsApi
      .getClaim(this.claimId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (claim) => this.state.set({ status: 'loaded', claim }),
        error: (error: ApiError) => {
          this.state.set(
            error.status === 404 ? { status: 'not-found' } : { status: 'error', error },
          );
        },
      });
  }
}
