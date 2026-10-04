using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.WorkManagement.Dtos;

public sealed record AssignClaimRequest(Guid OfficerId);

public sealed record ClaimAssignmentResponse(
    Guid ClaimId,
    Guid AssignedOfficerId,
    ClaimStatus Status,
    DateTimeOffset UpdatedAt);
