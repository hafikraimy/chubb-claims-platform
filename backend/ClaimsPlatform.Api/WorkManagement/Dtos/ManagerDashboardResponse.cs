namespace ClaimsPlatform.Api.WorkManagement.Dtos;

public sealed record ManagerDashboardResponse(
    IReadOnlyCollection<WorkClaimSummaryResponse> Claims,
    IReadOnlyCollection<OfficerWorkloadResponse> Officers,
    IReadOnlyCollection<ExposureResponse> Exposure,
    TeamPerformanceResponse Performance);

public sealed record OfficerWorkloadResponse(
    Guid OfficerId,
    string OfficerName,
    int OpenClaimCount,
    int InReviewCount,
    int AwaitingInfoCount,
    decimal? AverageOpenClaimAgeDays,
    decimal? OldestOpenClaimAgeDays);

public sealed record ExposureResponse(
    string Currency,
    decimal Amount);

public sealed record TeamPerformanceResponse(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    int TotalDecisions,
    decimal? AverageDecisionHours,
    IReadOnlyCollection<OfficerPerformanceResponse> Officers);

public sealed record OfficerPerformanceResponse(
    Guid OfficerId,
    string OfficerName,
    int SettledCount,
    int RejectedCount,
    int TotalDecisions,
    decimal? AverageDecisionHours);
