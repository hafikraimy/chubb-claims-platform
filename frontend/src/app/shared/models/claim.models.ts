export enum ClaimType {
  Motor = 'Motor',
  Property = 'Property',
}

export enum ClaimStatus {
  Submitted = 'Submitted',
  InReview = 'InReview',
  AwaitingInfo = 'AwaitingInfo',
  Settled = 'Settled',
  Rejected = 'Rejected',
}

export enum ClaimHistoryEventType {
  Submitted = 'Submitted',
  Assigned = 'Assigned',
  Reassigned = 'Reassigned',
  AssessedLossUpdated = 'AssessedLossUpdated',
  InformationRequested = 'InformationRequested',
  InformationProvided = 'InformationProvided',
  Settled = 'Settled',
  Rejected = 'Rejected',
}

export interface ClaimSummary {
  id: string;
  referenceNumber: string;
  type: ClaimType;
  incidentDate: string;
  reportedLossAmount: number;
  currency: string;
  status: ClaimStatus;
  submittedAt: string;
}

export interface ClaimDetail {
  id: string;
  referenceNumber: string;
  type: ClaimType;
  policyNumber: string;
  market: string;
  currency: string;
  incidentDate: string;
  incidentLocation: string;
  description: string;
  reportedLossAmount: number;
  assessedLossAmount: number | null;
  status: ClaimStatus;
  assignedOfficerId: string | null;
  submittedAt: string;
  updatedAt: string;
  decisionReason: string | null;
  settlementAmount: number | null;
  informationRequests: InformationRequest[];
  history: ClaimHistoryEntry[];
}

export interface InformationRequest {
  id: string;
  requestedByOfficerId: string;
  question: string;
  requestedAt: string;
  response: string | null;
  respondedAt: string | null;
}

export interface ClaimHistoryEntry {
  id: string;
  actingUserId: string;
  eventType: ClaimHistoryEventType;
  description: string;
  occurredAt: string;
}

export interface SubmitClaimRequest {
  type: ClaimType;
  policyNumber: string;
  currency: string;
  incidentDate: string;
  incidentLocation: string;
  description: string;
  reportedLossAmount: number;
}

export interface ClaimCreated {
  id: string;
  referenceNumber: string;
  status: ClaimStatus;
}

export interface ClaimUpdated {
  id: string;
  status: ClaimStatus;
  updatedAt: string;
}

export interface RespondToInformationRequest {
  response: string;
}

export interface RecordAssessedLoss {
  amount: number;
}

export interface CreateInformationRequest {
  question: string;
}

export interface SettleClaim {
  settlementAmount: number;
  reason: string;
}

export interface RejectClaim {
  reason: string;
}

export interface InformationRequestCreated {
  id: string;
  claimStatus: ClaimStatus;
  requestedAt: string;
}
