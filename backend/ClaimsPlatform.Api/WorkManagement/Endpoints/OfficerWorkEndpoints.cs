using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using Microsoft.EntityFrameworkCore;
using DomainClaim = ClaimsPlatform.Api.Claims.Domain.Claim;

namespace ClaimsPlatform.Api.WorkManagement.Endpoints;

public static class OfficerWorkEndpoints
{
    public static IEndpointRouteBuilder MapOfficerWorkEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var workGroup = endpoints.MapGroup("/api/work")
            .RequireAuthorization(AccessPolicies.ClaimsOfficer);

        workGroup.MapGet("/queue", GetQueueAsync);
        workGroup.MapGet("/my-claims", GetMyClaimsAsync);
        workGroup.MapGet("/team-summary", GetTeamSummaryAsync);

        endpoints.MapPost(
                "/api/claims/{claimId:guid}/assign-to-me",
                AssignToMeAsync)
            .RequireAuthorization(AccessPolicies.ClaimsOfficer);

        return endpoints;
    }

    private static async Task<IResult> GetQueueAsync(
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var officerMarket = principal.GetMarket();

        var claims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Status == ClaimStatus.Submitted &&
                claim.AssignedOfficerId == null &&
                claim.Market == officerMarket)
            .OrderBy(claim => claim.SubmittedAt)
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

        return Results.Ok(claims);
    }

    private static async Task<IResult> GetTeamSummaryAsync(
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var teamId = principal.GetTeamId();
        var market = principal.GetMarket();
        var periodEnd = DateTimeOffset.UtcNow;
        var periodStart = periodEnd.AddDays(-30);

        var officers = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == UserRole.ClaimsOfficer &&
                user.TeamId == teamId &&
                user.Market == market)
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
                claim.Market == market &&
                claim.AssignedOfficerId != null &&
                officerIds.Contains(
                    claim.AssignedOfficerId ?? Guid.Empty) &&
                claim.Status != ClaimStatus.Settled &&
                claim.Status != ClaimStatus.Rejected)
            .Select(claim => new
            {
                OfficerId = claim.AssignedOfficerId!.Value,
                claim.Status,
                claim.SubmittedAt
            })
            .ToListAsync(cancellationToken);

        var workload = officers
            .Select(officer =>
            {
                var officerClaims = openClaims
                    .Where(claim => claim.OfficerId == officer.Id)
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

        return Results.Ok(new TeamSummaryResponse(
            workload,
            performance));
    }

    private static async Task<IResult> GetMyClaimsAsync(
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var officerId = principal.GetUserId();

        var claims = await dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.AssignedOfficerId == officerId &&
                claim.Status != ClaimStatus.Settled &&
                claim.Status != ClaimStatus.Rejected)
            .OrderByDescending(claim => claim.UpdatedAt)
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

        return Results.Ok(claims);
    }

    private static async Task<IResult> AssignToMeAsync(
        Guid claimId,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var officerId = principal.GetUserId();
        var officerMarket = principal.GetMarket();

        var claim = await dbContext.Claims.SingleOrDefaultAsync(
            candidate =>
                candidate.Id == claimId &&
                candidate.Market == officerMarket,
            cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        if (claim.Status != ClaimStatus.Submitted ||
            claim.AssignedOfficerId is not null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Claim is not available",
                detail: "The claim has already been assigned or progressed.");
        }

        claim.AssignTo(
            officerId: officerId,
            actingUserId: officerId,
            assignedAt: DateTimeOffset.UtcNow);

        dbContext.ClaimHistoryEntries.Add(claim.History.Last());

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToSummary(claim));
    }

    private static WorkClaimSummaryResponse ToSummary(DomainClaim claim)
    {
        return new WorkClaimSummaryResponse(
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
            claim.UpdatedAt);
    }
}
