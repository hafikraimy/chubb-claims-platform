import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ClaimUpdated,
  CreateInformationRequest,
  InformationRequestCreated,
  RecordAssessedLoss,
  RejectClaim,
  SettleClaim,
} from '../../shared/models/claim.models';

@Service()
export class ClaimWorkflowApiService {
  private readonly http = inject(HttpClient);

  recordAssessedLoss(claimId: string, request: RecordAssessedLoss): Observable<ClaimUpdated> {
    return this.http.post<ClaimUpdated>(`/api/claims/${claimId}/assessed-loss`, request);
  }

  requestInformation(
    claimId: string,
    request: CreateInformationRequest,
  ): Observable<InformationRequestCreated> {
    return this.http.post<InformationRequestCreated>(
      `/api/claims/${claimId}/information-requests`,
      request,
    );
  }

  settle(claimId: string, request: SettleClaim): Observable<ClaimUpdated> {
    return this.http.post<ClaimUpdated>(`/api/claims/${claimId}/settle`, request);
  }

  reject(claimId: string, request: RejectClaim): Observable<ClaimUpdated> {
    return this.http.post<ClaimUpdated>(`/api/claims/${claimId}/reject`, request);
  }
}
