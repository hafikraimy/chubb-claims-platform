using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.WorkManagement.Dtos;

public sealed record WorkClaimSummaryResponse(
    Guid Id,
    string ReferenceNumber,
    ClaimType Type,
    string Market,
    string Currency,
    DateOnly IncidentDate,
    decimal ReportedLossAmount,
    decimal? AssessedLossAmount,
    ClaimStatus Status,
    Guid? AssignedOfficerId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt);
