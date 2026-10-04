namespace ClaimsPlatform.Api.WorkManagement.Dtos;

public sealed record TeamSummaryResponse(
    IReadOnlyCollection<OfficerWorkloadResponse> Workload,
    TeamPerformanceResponse Performance);
