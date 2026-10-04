import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `
    <section class="empty-state">
      <h2>{{ title() }}</h2>
      <p>{{ message() }}</p>
      <ng-content />
    </section>
  `,
  styles: `
    .empty-state {
      padding: clamp(2rem, 6vw, 4rem);
      border: 1px dashed var(--color-border);
      border-radius: var(--radius-card);
      background: var(--color-surface);
      text-align: center;
    }

    h2 {
      margin: 0;
      font-size: 1.25rem;
    }

    p {
      max-width: 32rem;
      margin: 0.75rem auto 1.25rem;
      color: var(--color-text-muted);
      line-height: 1.5;
    }
  `,
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly message = input.required<string>();
}
