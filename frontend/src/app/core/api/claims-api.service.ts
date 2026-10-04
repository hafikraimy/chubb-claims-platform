import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ClaimCreated,
  ClaimDetail,
  ClaimSummary,
  ClaimUpdated,
  RespondToInformationRequest,
  SubmitClaimRequest,
} from '../../shared/models/claim.models';

@Service()
export class ClaimsApiService {
  private readonly http = inject(HttpClient);

  getClaims(): Observable<ClaimSummary[]> {
    return this.http.get<ClaimSummary[]>('/api/claims');
  }

  getClaim(claimId: string): Observable<ClaimDetail> {
    return this.http.get<ClaimDetail>(`/api/claims/${claimId}`);
  }

  submitClaim(request: SubmitClaimRequest): Observable<ClaimCreated> {
    return this.http.post<ClaimCreated>('/api/claims', request);
  }

  respondToInformationRequest(
    claimId: string,
    requestId: string,
    request: RespondToInformationRequest,
  ): Observable<ClaimUpdated> {
    return this.http.post<ClaimUpdated>(
      `/api/claims/${claimId}/information-requests/${requestId}/response`,
      request,
    );
  }
}
