import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ApiError } from '../../../core/api/api-error';
import { OfficerWorkApiService } from '../../../core/api/officer-work-api.service';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { LoadingIndicator } from '../../../shared/components/loading-indicator/loading-indicator';
import { TeamSummary } from '../../../shared/models/work-management.models';

type TeamState =
  | { status: 'loading' }
  | { status: 'loaded'; summary: TeamSummary }
  | { status: 'error'; error: ApiError };

@Component({
  selector: 'app-officer-team-page',
  imports: [DatePipe, DecimalPipe, EmptyState, LoadingIndicator, MatButtonModule],
  templateUrl: './officer-team-page.html',
  styleUrl: './officer-team-page.scss',
})
export class OfficerTeamPage {
  private readonly workApi = inject(OfficerWorkApiService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly state = signal<TeamState>({ status: 'loading' });

  constructor() {
    this.loadSummary();
  }

  protected loadSummary(): void {
    this.state.set({ status: 'loading' });
    this.workApi
      .getTeamSummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => this.state.set({ status: 'loaded', summary }),
        error: (error: ApiError) => this.state.set({ status: 'error', error }),
      });
  }
}
