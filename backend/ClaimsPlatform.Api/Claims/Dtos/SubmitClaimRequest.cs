using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record SubmitClaimRequest(
    ClaimType Type,
    string PolicyNumber,
    string Currency,
    DateOnly IncidentDate,
    string IncidentLocation,
    string Description,
    decimal ReportedLossAmount);
