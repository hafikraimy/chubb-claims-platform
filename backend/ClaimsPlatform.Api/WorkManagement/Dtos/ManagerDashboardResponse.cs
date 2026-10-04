namespace ClaimsPlatform.Api.WorkManagement.Dtos;

public sealed record ManagerDashboardResponse(
    IReadOnlyCollection<WorkClaimSummaryResponse> Claims,
    IReadOnlyCollection<OfficerWorkloadResponse> Officers,
    IReadOnlyCollection<ExposureResponse> Exposure);

public sealed record OfficerWorkloadResponse(
    Guid OfficerId,
    string OfficerName,
    int OpenClaimCount,
    int InReviewCount,
    int AwaitingInfoCount);

public sealed record ExposureResponse(
    string Currency,
    decimal Amount);
