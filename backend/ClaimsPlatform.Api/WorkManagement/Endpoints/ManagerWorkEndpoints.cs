using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.WorkManagement.Endpoints;

public static class ManagerWorkEndpoints
{
    public static IEndpointRouteBuilder MapManagerWorkEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var managerGroup = endpoints.MapGroup("/api/manager")
            .RequireAuthorization(AccessPolicies.Manager);

        managerGroup.MapGet("/dashboard", GetDashboardAsync);

        endpoints.MapPost(
                "/api/claims/{claimId:guid}/assignments",
                AssignClaimAsync)
            .RequireAuthorization(AccessPolicies.Manager);

        return endpoints;
    }

    private static async Task<IResult> GetDashboardAsync(
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var managerTeamId = principal.GetTeamId();
        var managerMarket = principal.GetMarket();
        var periodEnd = DateTimeOffset.UtcNow;
        var periodStart = periodEnd.AddDays(-30);

        var officers = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == UserRole.ClaimsOfficer &&
                user.TeamId == managerTeamId &&
                user.Market == managerMarket)
            .OrderBy(user => user.Name)
            .Select(user => new
            {
                user.Id,
                user.Name
            })
            .ToListAsync(cancellationToken);

        var officerIds = officers
            .Select(officer => officer.Id)
            .ToArray();

        var openClaims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Market == managerMarket &&
                claim.Status != ClaimStatus.Settled &&
                claim.Status != ClaimStatus.Rejected &&
                (claim.AssignedOfficerId == null ||
                 officerIds.Contains(
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

        var workload = officers
            .Select(officer =>
            {
                var officerClaims = openClaims
                    .Where(claim =>
                        claim.AssignedOfficerId == officer.Id)
                    .ToArray();

                var claimAges = officerClaims
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
                    claimAges.Length == 0
                        ? null
                        : decimal.Round(claimAges.Average(), 1),
                    claimAges.Length == 0
                        ? null
                        : decimal.Round(claimAges.Max(), 1));
            })
            .ToArray();

        var exposure = openClaims
            .GroupBy(claim => claim.Currency)
            .OrderBy(group => group.Key)
            .Select(group => new ExposureResponse(
                group.Key,
                group.Sum(claim =>
                    claim.AssessedLossAmount ??
                    claim.ReportedLossAmount)))
            .ToArray();

        var completedClaims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Market == managerMarket &&
                claim.AssignedOfficerId != null &&
                officerIds.Contains(
                    claim.AssignedOfficerId ?? Guid.Empty) &&
                (claim.Status == ClaimStatus.Settled ||
                 claim.Status == ClaimStatus.Rejected) &&
                claim.UpdatedAt >= periodStart &&
                claim.UpdatedAt <= periodEnd)
            .Select(claim => new
            {
                OfficerId = claim.AssignedOfficerId!.Value,
                claim.Status,
                claim.SubmittedAt,
                DecidedAt = claim.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var officerPerformance = officers
            .Select(officer =>
            {
                var decisions = completedClaims
                    .Where(claim => claim.OfficerId == officer.Id)
                    .ToArray();

                var decisionHours = decisions
                    .Select(claim =>
                        (decimal)(claim.DecidedAt - claim.SubmittedAt)
                        .TotalHours)
                    .ToArray();

                return new OfficerPerformanceResponse(
                    officer.Id,
                    officer.Name,
                    decisions.Count(claim =>
                        claim.Status == ClaimStatus.Settled),
                    decisions.Count(claim =>
                        claim.Status == ClaimStatus.Rejected),
                    decisions.Length,
                    decisionHours.Length == 0
                        ? null
                        : decimal.Round(decisionHours.Average(), 1));
            })
            .ToArray();

        var allDecisionHours = completedClaims
            .Select(claim =>
                (decimal)(claim.DecidedAt - claim.SubmittedAt).TotalHours)
            .ToArray();

        var performance = new TeamPerformanceResponse(
            periodStart,
            periodEnd,
            completedClaims.Count,
            allDecisionHours.Length == 0
                ? null
                : decimal.Round(allDecisionHours.Average(), 1),
            officerPerformance);

        return Results.Ok(new ManagerDashboardResponse(
            openClaims,
            workload,
            exposure,
            performance));
    }

    private static async Task<IResult> AssignClaimAsync(
        Guid claimId,
        AssignClaimRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var managerId = principal.GetUserId();
        var managerTeamId = principal.GetTeamId();
        var managerMarket = principal.GetMarket();

        var officerExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user =>
                    user.Id == request.OfficerId &&
                    user.Role == UserRole.ClaimsOfficer &&
                    user.TeamId == managerTeamId &&
                    user.Market == managerMarket,
                cancellationToken);

        if (!officerExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid officer",
                detail: "The selected officer is not a member of your team.");
        }

        var claim = await dbContext.Claims.SingleOrDefaultAsync(
            candidate =>
                candidate.Id == claimId &&
                candidate.Market == managerMarket &&
                (candidate.AssignedOfficerId == null ||
                 dbContext.Users.Any(currentOfficer =>
                     currentOfficer.Id == candidate.AssignedOfficerId &&
                     currentOfficer.TeamId == managerTeamId)),
            cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        claim.AssignTo(
            officerId: request.OfficerId,
            actingUserId: managerId,
            assignedAt: DateTimeOffset.UtcNow);

        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new ClaimAssignmentResponse(
            claim.Id,
            request.OfficerId,
            claim.Status,
            claim.UpdatedAt));
    }
}
