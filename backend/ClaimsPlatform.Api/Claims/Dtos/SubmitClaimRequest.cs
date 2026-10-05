using System.ComponentModel.DataAnnotations;
using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record SubmitClaimRequest(
    ClaimType Type,
    [Required, MaxLength(ClaimFieldLimits.PolicyNumber)]
    string PolicyNumber,
    [Required, StringLength(ClaimFieldLimits.Currency, MinimumLength = ClaimFieldLimits.Currency)]
    string Currency,
    DateOnly IncidentDate,
    [Required, MaxLength(ClaimFieldLimits.IncidentLocation)]
    string IncidentLocation,
    [Required, MaxLength(ClaimFieldLimits.Description)]
    string Description,
    decimal ReportedLossAmount);
