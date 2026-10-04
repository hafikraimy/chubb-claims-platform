using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.WorkManagement.Services;

public sealed class ManagerWorkService(
    ClaimsDbContext dbContext,
    TeamReportingService teamReporting)
{
    public async Task<ManagerDashboardResponse> GetDashboardAsync(
        Guid teamId,
        string market,
        CancellationToken cancellationToken)
    {
        var teamOfficerIds = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == UserRole.ClaimsOfficer &&
                user.TeamId == teamId &&
                user.Market == market)
            .Select(user => user.Id)
            .ToArrayAsync(cancellationToken);

        var openClaims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Market == market &&
                claim.Status != ClaimStatus.Settled &&
                claim.Status != ClaimStatus.Rejected &&
                (claim.AssignedOfficerId == null ||
                 teamOfficerIds.Contains(
                     claim.AssignedOfficerId ?? Guid.Empty)))
            .OrderBy(claim => claim.AssignedOfficerId != null)
            .ThenBy(claim => claim.SubmittedAt)
            .Select(claim => new WorkClaimSummaryResponse(
                claim.Id,
                claim.ReferenceNumber,
                claim.Type,
                claim.Market,
                claim.Currency,
                claim.IncidentDate,
                claim.ReportedLossAmount,
                claim.AssessedLossAmount,
                claim.Status,
                claim.AssignedOfficerId,
                claim.SubmittedAt,
                claim.UpdatedAt))
            .ToListAsync(cancellationToken);

        var exposure = openClaims
            .GroupBy(claim => claim.Currency)
            .OrderBy(group => group.Key)
            .Select(group => new ExposureResponse(
                group.Key,
                group.Sum(claim =>
                    claim.AssessedLossAmount ??
                    claim.ReportedLossAmount)))
            .ToArray();

        var teamSummary = await teamReporting.GetAsync(
            teamId,
            market,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return new ManagerDashboardResponse(
            openClaims,
            teamSummary.Workload,
            exposure,
            teamSummary.Performance);
    }

    public async Task<ManagerAssignmentResult> AssignAsync(
        Guid claimId,
        Guid officerId,
        Guid managerId,
        Guid teamId,
        string market,
        CancellationToken cancellationToken)
    {
        var officerExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user =>
                    user.Id == officerId &&
                    user.Role == UserRole.ClaimsOfficer &&
                    user.TeamId == teamId &&
                    user.Market == market,
                cancellationToken);

        if (!officerExists)
        {
            return new ManagerAssignmentResult(
                ManagerAssignmentOutcome.InvalidOfficer,
                null);
        }

        var claim = await dbContext.Claims.SingleOrDefaultAsync(
            candidate =>
                candidate.Id == claimId &&
                candidate.Market == market &&
                (candidate.AssignedOfficerId == null ||
                 dbContext.Users.Any(currentOfficer =>
                     currentOfficer.Id == candidate.AssignedOfficerId &&
                     currentOfficer.TeamId == teamId)),
            cancellationToken);

        if (claim is null)
        {
            return new ManagerAssignmentResult(
                ManagerAssignmentOutcome.NotFound,
                null);
        }

        claim.AssignTo(officerId, managerId, DateTimeOffset.UtcNow);
        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ManagerAssignmentResult(
            ManagerAssignmentOutcome.Assigned,
            new ClaimAssignmentResponse(
                claim.Id,
                officerId,
                claim.Status,
                claim.UpdatedAt));
    }
}

public enum ManagerAssignmentOutcome
{
    Assigned,
    NotFound,
    InvalidOfficer
}

public sealed record ManagerAssignmentResult(
    ManagerAssignmentOutcome Outcome,
    ClaimAssignmentResponse? Assignment);
