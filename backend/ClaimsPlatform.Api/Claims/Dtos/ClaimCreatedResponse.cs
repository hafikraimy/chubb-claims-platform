using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record ClaimCreatedResponse(
    Guid Id,
    string ReferenceNumber,
    ClaimStatus Status);
