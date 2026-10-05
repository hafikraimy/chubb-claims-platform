import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { ApiError } from '../../../core/api/api-error';
import { ManagerApiService } from '../../../core/api/manager-api.service';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';
import { ManagerDashboard, WorkClaimSummary } from '../../../shared/models/work-management.models';

type DashboardState =
  | { status: 'loading' }
  | { status: 'loaded'; dashboard: ManagerDashboard }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-manager-home-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    DecimalPipe,
    EmptyState,
    LoadingIndicator,
    MatButtonModule,
    MatProgressSpinnerModule,
    RouterLink,
    StatusBadge,
  ],
  templateUrl: './manager-home-page.html',
  styleUrl: './manager-home-page.scss',
})
export class ManagerHomePage {
  private readonly managerApi = inject(ManagerApiService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly state = signal<DashboardState>({ status: 'loading' });
  protected readonly selections = signal<Record<string, string>>({});
  protected readonly assigningClaimId = signal<string | null>(null);
  protected readonly assignmentError = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  constructor() {
    this.loadDashboard();
  }

  protected loadDashboard(message?: string): void {
    this.state.set({ status: 'loading' });
    this.assignmentError.set(null);
    this.successMessage.set(null);
    this.managerApi
      .getDashboard()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (dashboard) => {
          this.selections.set(
            Object.fromEntries(
              dashboard.claims
                .filter((claim) => claim.assignedOfficerId)
                .map((claim) => [claim.id, claim.assignedOfficerId!]),
            ),
          );
          this.state.set({ status: 'loaded', dashboard });
          this.successMessage.set(message ?? null);
        },
        error: (error: ApiError) => this.state.set({ status: 'error', error }),
      });
  }

  protected selectOfficer(claimId: string, event: Event): void {
    this.selections.update((current) => ({
      ...current,
      [claimId]: (event.target as HTMLSelectElement).value,
    }));
  }

  protected assign(claim: WorkClaimSummary): void {
    const officerId = this.selections()[claim.id];
    if (!officerId || officerId === claim.assignedOfficerId || this.assigningClaimId()) return;
    this.assigningClaimId.set(claim.id);
    this.assignmentError.set(null);
    this.managerApi
      .assignClaim(claim.id, { officerId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.assigningClaimId.set(null);
          this.loadDashboard(`${claim.referenceNumber} assignment updated.`);
        },
        error: (error: ApiError) => {
          this.assigningClaimId.set(null);
          this.assignmentError.set(error.detail);
        },
      });
  }
}
