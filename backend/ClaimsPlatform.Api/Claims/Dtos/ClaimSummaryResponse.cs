using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record ClaimSummaryResponse(
    Guid Id,
    string ReferenceNumber,
    ClaimType Type,
    DateOnly IncidentDate,
    decimal ReportedLossAmount,
    string Currency,
    ClaimStatus Status,
    DateTimeOffset SubmittedAt);
