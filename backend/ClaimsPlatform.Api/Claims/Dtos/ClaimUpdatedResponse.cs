using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record ClaimUpdatedResponse(
    Guid Id,
    ClaimStatus Status,
    DateTimeOffset UpdatedAt);

public sealed record InformationRequestCreatedResponse(
    Guid Id,
    ClaimStatus ClaimStatus,
    DateTimeOffset RequestedAt);
