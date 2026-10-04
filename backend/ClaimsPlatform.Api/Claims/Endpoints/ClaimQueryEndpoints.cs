using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Claims.Endpoints;

public static class ClaimQueryEndpoints
{
    public static IEndpointRouteBuilder MapClaimQueryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/claims/{claimId:guid}",
                GetClaimAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> GetClaimAsync(
        Guid claimId,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Claims
            .AsNoTracking()
            .AsSplitQuery()
            .Include(claim => claim.InformationRequests)
            .Include(claim => claim.History)
            .Where(claim => claim.Id == claimId);

        if (principal.IsInRole(UserRole.Claimant.ToString()))
        {
            var claimantId = principal.GetUserId();
            query = query.Where(claim => claim.ClaimantId == claimantId);
        }
        else if (principal.IsInRole(UserRole.ClaimsOfficer.ToString()))
        {
            var officerId = principal.GetUserId();
            var officerMarket = principal.GetMarket();

            query = query.Where(claim =>
                claim.AssignedOfficerId == officerId ||
                (claim.Status == ClaimStatus.Submitted &&
                 claim.AssignedOfficerId == null &&
                 claim.Market == officerMarket));
        }
        else if (principal.IsInRole(UserRole.Manager.ToString()))
        {
            var managerTeamId = principal.GetTeamId();
            var managerMarket = principal.GetMarket();

            query = query.Where(claim =>
                claim.Market == managerMarket &&
                (claim.AssignedOfficerId == null ||
                 dbContext.Users.Any(officer =>
                     officer.Id == claim.AssignedOfficerId &&
                     officer.TeamId == managerTeamId)));
        }
        else
        {
            return Results.Forbid();
        }

        var claim = await query.SingleOrDefaultAsync(cancellationToken);

        return claim is null
            ? Results.NotFound()
            : Results.Ok(claim.ToDetailResponse());
    }
}
