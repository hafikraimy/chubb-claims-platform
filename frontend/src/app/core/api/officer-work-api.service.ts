import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { TeamSummary, WorkClaimSummary } from '../../shared/models/work-management.models';

@Service()
export class OfficerWorkApiService {
  private readonly http = inject(HttpClient);

  getQueue(): Observable<WorkClaimSummary[]> {
    return this.http.get<WorkClaimSummary[]>('/api/work/queue');
  }

  getMyClaims(): Observable<WorkClaimSummary[]> {
    return this.http.get<WorkClaimSummary[]>('/api/work/my-claims');
  }

  getTeamSummary(): Observable<TeamSummary> {
    return this.http.get<TeamSummary>('/api/work/team-summary');
  }

  assignToMe(claimId: string): Observable<WorkClaimSummary> {
    return this.http.post<WorkClaimSummary>(`/api/claims/${claimId}/assign-to-me`, {});
  }
}
