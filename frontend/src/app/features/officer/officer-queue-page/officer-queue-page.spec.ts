import { TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { OfficerWorkApiService } from '../../../core/api/officer-work-api.service';
import { ClaimStatus, ClaimType } from '../../../shared/models/claim.models';
import { WorkClaimSummary } from '../../../shared/models/work-management.models';
import { OfficerQueuePage } from './officer-queue-page';

const claim: WorkClaimSummary = {
  id: 'claim-1',
  referenceNumber: 'MY-2026-001',
  type: ClaimType.Motor,
  market: 'MY',
  currency: 'MYR',
  incidentDate: '2026-10-01',
  reportedLossAmount: 2500,
  assessedLossAmount: null,
  status: ClaimStatus.Submitted,
  assignedOfficerId: null,
  submittedAt: '2026-10-01T01:30:00Z',
  updatedAt: '2026-10-01T01:30:00Z',
};

describe('OfficerQueuePage', () => {
  const officerWorkApi = {
    getQueue: vi.fn(),
    assignToMe: vi.fn(),
  };

  beforeEach(async () => {
    officerWorkApi.getQueue.mockReset();
    officerWorkApi.assignToMe.mockReset();

    await TestBed.configureTestingModule({
      imports: [OfficerQueuePage],
      providers: [{ provide: OfficerWorkApiService, useValue: officerWorkApi }],
    }).compileComponents();
  });

  it('shows a loading state while the queue is requested', () => {
    officerWorkApi.getQueue.mockReturnValue(new Subject<WorkClaimSummary[]>());

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading unassigned claims');
  });

  it('renders submitted claims with an assignment action', () => {
    officerWorkApi.getQueue.mockReturnValue(of([claim]));

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();

    const table = fixture.nativeElement.querySelector('table') as HTMLTableElement;
    expect(table.textContent).toContain('MY-2026-001');
    expect(table.textContent).toContain('Submitted');
    expect(table.textContent).toContain('MYR');
    expect(table.querySelector('button')?.textContent).toContain('Assign to me');
  });

  it('shows a clear state when no claims are waiting', () => {
    officerWorkApi.getQueue.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Queue is clear');
  });

  it('assigns a claim, removes it from the queue, and confirms success', () => {
    officerWorkApi.getQueue.mockReturnValue(of([claim]));
    officerWorkApi.assignToMe.mockReturnValue(
      of({ ...claim, status: ClaimStatus.InReview, assignedOfficerId: 'officer-my-1' }),
    );

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('table button') as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(officerWorkApi.assignToMe).toHaveBeenCalledWith('claim-1');
    expect(fixture.nativeElement.textContent).toContain('MY-2026-001 is now in your work.');
    expect(fixture.nativeElement.textContent).toContain('Queue is clear');
  });

  it('locks assignment actions while a pickup is pending', () => {
    const assignment = new Subject<WorkClaimSummary>();
    officerWorkApi.getQueue.mockReturnValue(of([claim]));
    officerWorkApi.assignToMe.mockReturnValue(assignment);

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('table button') as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(button.disabled).toBe(true);
    expect(button.textContent).toContain('Assigning');
  });

  it('shows an assignment conflict and reloads the queue', () => {
    const error = new ApiError(
      409,
      'Claim unavailable',
      'This claim has already been assigned.',
      {},
      null,
    );
    officerWorkApi.getQueue.mockReturnValue(of([claim]));
    officerWorkApi.assignToMe.mockReturnValue(throwError(() => error));

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();

    const assignButton = fixture.nativeElement.querySelector('table button') as HTMLButtonElement;
    assignButton.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('This claim has already been assigned.');

    const reloadButton = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((button) => button.textContent?.includes('Reload queue'));
    reloadButton?.click();
    fixture.detectChanges();

    expect(officerWorkApi.getQueue).toHaveBeenCalledTimes(2);
  });

  it('shows a queue error and retries loading', () => {
    const error = new ApiError(500, 'Failed', 'Please try later.', {}, null);
    officerWorkApi.getQueue
      .mockReturnValueOnce(throwError(() => error))
      .mockReturnValueOnce(of([claim]));

    const fixture = TestBed.createComponent(OfficerQueuePage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Please try later.');

    const retry = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    retry.click();
    fixture.detectChanges();

    expect(officerWorkApi.getQueue).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('MY-2026-001');
  });
});
