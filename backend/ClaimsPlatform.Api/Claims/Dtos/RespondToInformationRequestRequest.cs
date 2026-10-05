using System.ComponentModel.DataAnnotations;
using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record RespondToInformationRequestRequest(
    [Required, MaxLength(ClaimFieldLimits.InformationResponse)]
    string Response);
