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
            .Select(officer => new OfficerWorkloadResponse(
                officer.Id,
                officer.Name,
                openClaims.Count(claim =>
                    claim.AssignedOfficerId == officer.Id),
                openClaims.Count(claim =>
                    claim.AssignedOfficerId == officer.Id &&
                    claim.Status == ClaimStatus.InReview),
                openClaims.Count(claim =>
                    claim.AssignedOfficerId == officer.Id &&
                    claim.Status == ClaimStatus.AwaitingInfo)))
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

        return Results.Ok(new ManagerDashboardResponse(
            openClaims,
            workload,
            exposure));
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
