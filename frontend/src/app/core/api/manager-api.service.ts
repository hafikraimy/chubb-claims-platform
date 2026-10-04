import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AssignClaimRequest,
  ClaimAssignment,
  ManagerDashboard,
} from '../../shared/models/work-management.models';

@Service()
export class ManagerApiService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<ManagerDashboard> {
    return this.http.get<ManagerDashboard>('/api/manager/dashboard');
  }

  assignClaim(claimId: string, request: AssignClaimRequest): Observable<ClaimAssignment> {
    return this.http.post<ClaimAssignment>(`/api/claims/${claimId}/assignments`, request);
  }
}
