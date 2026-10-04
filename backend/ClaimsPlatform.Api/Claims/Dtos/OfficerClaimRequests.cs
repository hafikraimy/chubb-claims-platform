namespace ClaimsPlatform.Api.Claims.Dtos;

public sealed record RecordAssessedLossRequest(decimal Amount);

public sealed record CreateInformationRequest(string Question);

public sealed record SettleClaimRequest(
    decimal SettlementAmount,
    string Reason);

public sealed record RejectClaimRequest(string Reason);
