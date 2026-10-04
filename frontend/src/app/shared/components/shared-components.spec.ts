import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ClaimStatus } from '../models/claim.models';
import { EmptyState } from './empty-state/empty-state';
import { LoadingIndicator } from './loading-indicator/loading-indicator';
import { StatusBadge } from './status-badge/status-badge';

@Component({
  imports: [EmptyState],
  template: `
    <app-empty-state title="No claims" message="Report an incident to begin.">
      <button type="button">Report incident</button>
    </app-empty-state>
  `,
})
class EmptyStateHost {}

describe('shared components', () => {
  it.each([
    [ClaimStatus.Submitted, 'Submitted', 'status-badge--info'],
    [ClaimStatus.InReview, 'In review', 'status-badge--info'],
    [ClaimStatus.AwaitingInfo, 'Awaiting information', 'status-badge--awaiting'],
    [ClaimStatus.Settled, 'Settled', 'status-badge--settled'],
    [ClaimStatus.Rejected, 'Rejected', 'status-badge--rejected'],
  ])('renders the %s status', (status, label, cssClass) => {
    const fixture = TestBed.createComponent(StatusBadge);
    fixture.componentRef.setInput('status', status);
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.status-badge');
    expect(badge.textContent.trim()).toBe(label);
    expect(badge.classList.contains(cssClass)).toBe(true);
  });

  it('announces loading progress', () => {
    const fixture = TestBed.createComponent(LoadingIndicator);
    fixture.componentRef.setInput('label', 'Loading claims…');
    fixture.detectChanges();

    const status = fixture.nativeElement.querySelector('[role="status"]');
    expect(status.textContent).toContain('Loading claims…');
  });

  it('renders empty-state content and a projected action', () => {
    const fixture = TestBed.createComponent(EmptyStateHost);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('h2').textContent).toContain('No claims');
    expect(fixture.nativeElement.querySelector('button').textContent).toContain('Report incident');
  });
});
