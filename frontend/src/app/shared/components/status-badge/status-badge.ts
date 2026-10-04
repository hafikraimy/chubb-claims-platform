import { Component, computed, input } from '@angular/core';
import { ClaimStatus } from '../../models/claim.models';

@Component({
  selector: 'app-status-badge',
  template: `
    <span class="status-badge" [class]="toneClass()">
      {{ label() }}
    </span>
  `,
  styleUrl: './status-badge.scss',
})
export class StatusBadge {
  readonly status = input.required<ClaimStatus>();

  protected readonly label = computed(() => {
    switch (this.status()) {
      case ClaimStatus.Submitted:
        return 'Submitted';
      case ClaimStatus.InReview:
        return 'In review';
      case ClaimStatus.AwaitingInfo:
        return 'Awaiting information';
      case ClaimStatus.Settled:
        return 'Settled';
      case ClaimStatus.Rejected:
        return 'Rejected';
    }
  });

  protected readonly toneClass = computed(() => {
    switch (this.status()) {
      case ClaimStatus.Submitted:
      case ClaimStatus.InReview:
        return 'status-badge--info';
      case ClaimStatus.AwaitingInfo:
        return 'status-badge--awaiting';
      case ClaimStatus.Settled:
        return 'status-badge--settled';
      case ClaimStatus.Rejected:
        return 'status-badge--rejected';
    }
  });
}
