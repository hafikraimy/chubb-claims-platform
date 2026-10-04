import { Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-indicator',
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="loading" role="status">
      <mat-spinner diameter="28" aria-hidden="true" />
      <span>{{ label() }}</span>
    </div>
  `,
  styles: `
    .loading {
      display: flex;
      gap: 0.75rem;
      align-items: center;
      justify-content: center;
      min-height: 8rem;
      color: var(--color-text-muted);
    }
  `,
})
export class LoadingIndicator {
  readonly label = input('Loading…');
}
