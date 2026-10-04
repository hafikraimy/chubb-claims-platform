using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record ClaimDetailResponse(
    Guid Id,
    string ReferenceNumber,
    ClaimType Type,
    string PolicyNumber,
    string Market,
    string Currency,
    DateOnly IncidentDate,
    string IncidentLocation,
    string Description,
    decimal ReportedLossAmount,
    decimal? AssessedLossAmount,
    ClaimStatus Status,
    Guid? AssignedOfficerId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt,
    string? DecisionReason,
    decimal? SettlementAmount,
    IReadOnlyCollection<InformationRequestResponse> InformationRequests,
    IReadOnlyCollection<ClaimHistoryResponse> History);

public sealed record InformationRequestResponse(
    Guid Id,
    Guid RequestedByOfficerId,
    string Question,
    DateTimeOffset RequestedAt,
    string? Response,
    DateTimeOffset? RespondedAt);

public sealed record ClaimHistoryResponse(
    Guid Id,
    Guid ActingUserId,
    ClaimHistoryEventType EventType,
    string Description,
    DateTimeOffset OccurredAt);
