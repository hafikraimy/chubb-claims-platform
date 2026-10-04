import { Component } from '@angular/core';

@Component({
  selector: 'app-claimant-home-page',
  template: `
    <section class="landing" aria-labelledby="claimant-title">
      <p class="eyebrow">Claimant workspace</p>
      <h1 id="claimant-title">Your claims journey starts here</h1>
      <p>
        Claim submission, tracking, information responses, and decisions will be added in the next
        claimant feature slices.
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
export class ClaimantHomePage {}
