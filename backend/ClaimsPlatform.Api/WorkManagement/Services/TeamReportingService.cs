using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.WorkManagement.Services;

public sealed class TeamReportingService(ClaimsDbContext dbContext)
{
    public async Task<TeamSummaryResponse> GetAsync(
        Guid teamId,
        string market,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken)
    {
        var periodStart = periodEnd.AddDays(-30);

        var officers = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == UserRole.ClaimsOfficer &&
                user.TeamId == teamId &&
                user.Market == market)
            .OrderBy(user => user.Name)
            .Select(user => new OfficerReference(user.Id, user.Name))
            .ToListAsync(cancellationToken);

        var officerIds = officers
            .Select(officer => officer.Id)
            .ToArray();

        var openClaims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Market == market &&
                claim.AssignedOfficerId != null &&
                officerIds.Contains(
                    claim.AssignedOfficerId ?? Guid.Empty) &&
                claim.Status != ClaimStatus.Settled &&
                claim.Status != ClaimStatus.Rejected)
            .Select(claim => new OpenClaimMetric(
                claim.AssignedOfficerId!.Value,
                claim.Status,
                claim.SubmittedAt))
            .ToListAsync(cancellationToken);

        var workload = officers
            .Select(officer => ToWorkload(
                officer,
                openClaims,
                periodEnd))
            .ToArray();

        var completedClaims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Market == market &&
                claim.AssignedOfficerId != null &&
                officerIds.Contains(
                    claim.AssignedOfficerId ?? Guid.Empty) &&
                (claim.Status == ClaimStatus.Settled ||
                 claim.Status == ClaimStatus.Rejected) &&
                claim.UpdatedAt >= periodStart &&
                claim.UpdatedAt <= periodEnd)
            .Select(claim => new CompletedClaimMetric(
                claim.AssignedOfficerId!.Value,
                claim.Status,
                claim.SubmittedAt,
                claim.UpdatedAt))
            .ToListAsync(cancellationToken);

        var officerPerformance = officers
            .Select(officer => ToPerformance(
                officer,
                completedClaims))
            .ToArray();

        var allDecisionHours = completedClaims
            .Select(claim =>
                (decimal)(claim.DecidedAt - claim.SubmittedAt).TotalHours)
            .ToArray();

        var performance = new TeamPerformanceResponse(
            periodStart,
            periodEnd,
            completedClaims.Count,
            AverageOrNull(allDecisionHours),
            officerPerformance);

        return new TeamSummaryResponse(workload, performance);
    }

    private static OfficerWorkloadResponse ToWorkload(
        OfficerReference officer,
        IReadOnlyCollection<OpenClaimMetric> openClaims,
        DateTimeOffset periodEnd)
    {
        var officerClaims = openClaims
            .Where(claim => claim.OfficerId == officer.Id)
            .ToArray();

        var ages = officerClaims
            .Select(claim =>
                (decimal)(periodEnd - claim.SubmittedAt).TotalDays)
            .ToArray();

        return new OfficerWorkloadResponse(
            officer.Id,
            officer.Name,
            officerClaims.Length,
            officerClaims.Count(claim =>
                claim.Status == ClaimStatus.InReview),
            officerClaims.Count(claim =>
                claim.Status == ClaimStatus.AwaitingInfo),
            AverageOrNull(ages),
            ages.Length == 0 ? null : decimal.Round(ages.Max(), 1));
    }

    private static OfficerPerformanceResponse ToPerformance(
        OfficerReference officer,
        IReadOnlyCollection<CompletedClaimMetric> completedClaims)
    {
        var decisions = completedClaims
            .Where(claim => claim.OfficerId == officer.Id)
            .ToArray();

        var decisionHours = decisions
            .Select(claim =>
                (decimal)(claim.DecidedAt - claim.SubmittedAt).TotalHours)
            .ToArray();

        return new OfficerPerformanceResponse(
            officer.Id,
            officer.Name,
            decisions.Count(claim => claim.Status == ClaimStatus.Settled),
            decisions.Count(claim => claim.Status == ClaimStatus.Rejected),
            decisions.Length,
            AverageOrNull(decisionHours));
    }

    private static decimal? AverageOrNull(decimal[] values)
    {
        return values.Length == 0
            ? null
            : decimal.Round(values.Average(), 1);
    }

    private sealed record OfficerReference(Guid Id, string Name);

    private sealed record OpenClaimMetric(
        Guid OfficerId,
        ClaimStatus Status,
        DateTimeOffset SubmittedAt);

    private sealed record CompletedClaimMetric(
        Guid OfficerId,
        ClaimStatus Status,
        DateTimeOffset SubmittedAt,
        DateTimeOffset DecidedAt);
}
