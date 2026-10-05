using System.ComponentModel.DataAnnotations;
using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record RecordAssessedLossRequest(decimal Amount);

public sealed record CreateInformationRequest(
    [Required, MaxLength(ClaimFieldLimits.InformationQuestion)]
    string Question);

public sealed record SettleClaimRequest(
    decimal SettlementAmount,
    [Required, MaxLength(ClaimFieldLimits.DecisionReason)]
    string Reason);

public sealed record RejectClaimRequest(
    [Required, MaxLength(ClaimFieldLimits.DecisionReason)]
    string Reason);
