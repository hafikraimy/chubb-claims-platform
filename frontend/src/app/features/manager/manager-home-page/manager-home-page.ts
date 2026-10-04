import { Component } from '@angular/core';

@Component({
  selector: 'app-manager-home-page',
  template: `
    <section class="landing" aria-labelledby="manager-title">
      <p class="eyebrow">Manager workspace</p>
      <h1 id="manager-title">See team workload and exposure</h1>
      <p>
        Dashboard metrics, performance, outstanding exposure, and assignment controls will be added
        in the manager feature slice.
      </p>
    </section>
  `,
  styles: `
    .landing {
      max-width: 48rem;
      padding: clamp(1.5rem, 4vw, 3rem);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-card);
      background: var(--color-surface);
      box-shadow: var(--shadow-card);
    }

    .eyebrow {
      margin: 0 0 0.5rem;
      color: var(--color-primary);
      font-size: 0.75rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h1 {
      margin: 0;
      font-size: clamp(1.75rem, 4vw, 2.5rem);
      line-height: 1.15;
    }

    p:last-child {
      margin: 1rem 0 0;
      color: var(--color-text-muted);
      line-height: 1.6;
    }
  `,
})
export class ManagerHomePage {}
