import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { ApiError } from '../../../core/api/api-error';
import { OfficerWorkApiService } from '../../../core/api/officer-work-api.service';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { WorkClaimSummary } from '../../../shared/models/work-management.models';

type WorkListState =
  | { status: 'loading' }
  | { status: 'loaded'; claims: WorkClaimSummary[] }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-officer-work-list-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    EmptyState,
    LoadingIndicator,
    MatButtonModule,
    RouterLink,
    StatusBadge,
  ],
  templateUrl: './officer-work-list-page.html',
  styleUrl: './officer-work-list-page.scss',
})
export class OfficerWorkListPage {
  private readonly officerWorkApi = inject(OfficerWorkApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly state = signal<WorkListState>({ status: 'loading' });

  constructor() {
    this.loadClaims();
  }

  protected loadClaims(): void {
    this.state.set({ status: 'loading' });

    this.officerWorkApi
      .getMyClaims()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (claims) => this.state.set({ status: 'loaded', claims }),
        error: (error: ApiError) => this.state.set({ status: 'error', error }),
      });
  }
}
