using DomainClaim = ClaimsPlatform.Api.Claims.Domain.Claim;

namespace ClaimsPlatform.Api.Claims.Dtos;

public static class ClaimDetailMapper
{
    public static ClaimDetailResponse ToDetailResponse(
        this DomainClaim claim)
    {
        var informationRequests = claim.InformationRequests
            .OrderByDescending(request => request.RequestedAt)
            .Select(request => new InformationRequestResponse(
                request.Id,
                request.RequestedByOfficerId,
                request.Question,
                request.RequestedAt,
                request.Response,
                request.RespondedAt))
            .ToArray();

        var history = claim.History
            .OrderBy(entry => entry.OccurredAt)
            .Select(entry => new ClaimHistoryResponse(
                entry.Id,
                entry.ActingUserId,
                entry.EventType,
                entry.Description,
                entry.OccurredAt))
            .ToArray();

        return new ClaimDetailResponse(
            claim.Id,
            claim.ReferenceNumber,
            claim.Type,
            claim.PolicyNumber,
            claim.Market,
            claim.Currency,
            claim.IncidentDate,
            claim.IncidentLocation,
            claim.Description,
            claim.ReportedLossAmount,
            claim.AssessedLossAmount,
            claim.Status,
            claim.AssignedOfficerId,
            claim.SubmittedAt,
            claim.UpdatedAt,
            claim.DecisionReason,
            claim.SettlementAmount,
            informationRequests,
            history);
    }
}
