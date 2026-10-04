import { ClaimStatus, ClaimType } from './claim.models';

export interface WorkClaimSummary {
  id: string;
  referenceNumber: string;
  type: ClaimType;
  market: string;
  currency: string;
  incidentDate: string;
  reportedLossAmount: number;
  assessedLossAmount: number | null;
  status: ClaimStatus;
  assignedOfficerId: string | null;
  submittedAt: string;
  updatedAt: string;
}

export interface OfficerWorkload {
  officerId: string;
  officerName: string;
  openClaimCount: number;
  inReviewCount: number;
  awaitingInfoCount: number;
  averageOpenClaimAgeDays: number | null;
  oldestOpenClaimAgeDays: number | null;
}

export interface Exposure {
  currency: string;
  amount: number;
}

export interface OfficerPerformance {
  officerId: string;
  officerName: string;
  settledCount: number;
  rejectedCount: number;
  totalDecisions: number;
  averageDecisionHours: number | null;
}

export interface TeamPerformance {
  periodStart: string;
  periodEnd: string;
  totalDecisions: number;
  averageDecisionHours: number | null;
  officers: OfficerPerformance[];
}

export interface TeamSummary {
  workload: OfficerWorkload[];
  performance: TeamPerformance;
}

export interface ManagerDashboard {
  claims: WorkClaimSummary[];
  officers: OfficerWorkload[];
  exposure: Exposure[];
  performance: TeamPerformance;
}

export interface AssignClaimRequest {
  officerId: string;
}

export interface ClaimAssignment {
  claimId: string;
  assignedOfficerId: string;
  status: ClaimStatus;
  updatedAt: string;
}
