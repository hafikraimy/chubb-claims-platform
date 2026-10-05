import { TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { ApiError } from '../../../core/api/api-error';
import { OfficerWorkApiService } from '../../../core/api/officer-work-api.service';
import { TeamSummary } from '../../../shared/models/work-management.models';
import { OfficerTeamPage } from './officer-team-page';

const summary: TeamSummary = {
  workload: [
    {
      officerId: 'officer-1',
      officerName: 'Aisha Tan',
      openClaimCount: 4,
      inReviewCount: 3,
      awaitingInfoCount: 1,
      averageOpenClaimAgeDays: 2.5,
      oldestOpenClaimAgeDays: 5,
    },
  ],
  performance: {
    periodStart: '2026-09-05T00:00:00Z',
    periodEnd: '2026-10-05T00:00:00Z',
    totalDecisions: 6,
    averageDecisionHours: 18.5,
    officers: [
      {
        officerId: 'officer-1',
        officerName: 'Aisha Tan',
        settledCount: 4,
        rejectedCount: 2,
        totalDecisions: 6,
        averageDecisionHours: 18.5,
      },
    ],
  },
};

describe('OfficerTeamPage', () => {
  const workApi = { getTeamSummary: vi.fn() };
  beforeEach(async () => {
    workApi.getTeamSummary.mockReset();
    await TestBed.configureTestingModule({
      imports: [OfficerTeamPage],
      providers: [{ provide: OfficerWorkApiService, useValue: workApi }],
    }).compileComponents();
  });

  it('shows loading while requesting the summary', () => {
    workApi.getTeamSummary.mockReturnValue(new Subject<TeamSummary>());
    const fixture = TestBed.createComponent(OfficerTeamPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Loading team summary');
  });

  it('renders workload and performance without management actions', () => {
    workApi.getTeamSummary.mockReturnValue(of(summary));
    const fixture = TestBed.createComponent(OfficerTeamPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Aisha Tan');
    expect(fixture.nativeElement.textContent).toContain('Total decisions');
    expect(fixture.nativeElement.textContent).toContain('Read only');
    expect(fixture.nativeElement.textContent).not.toContain('Assign claim');
  });

  it('shows an empty team state', () => {
    workApi.getTeamSummary.mockReturnValue(of({ ...summary, workload: [] }));
    const fixture = TestBed.createComponent(OfficerTeamPage);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No team data');
  });

  it('retries after an API error', () => {
    workApi.getTeamSummary
      .mockReturnValueOnce(throwError(() => new ApiError(500, 'Failed', 'Try later.', {}, null)))
      .mockReturnValueOnce(of(summary));
    const fixture = TestBed.createComponent(OfficerTeamPage);
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(workApi.getTeamSummary).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.textContent).toContain('Aisha Tan');
  });
});
