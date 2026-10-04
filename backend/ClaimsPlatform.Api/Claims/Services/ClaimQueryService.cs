using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Claims.Services;

public sealed class ClaimQueryService(ClaimsDbContext dbContext)
{
    public async Task<ClaimDetailResponse?> GetAsync(
        Guid claimId,
        DemoUserContext user,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Claims
            .AsNoTracking()
            .AsSplitQuery()
            .Include(claim => claim.InformationRequests)
            .Include(claim => claim.History)
            .Where(claim => claim.Id == claimId);

        query = user.Role switch
        {
            UserRole.Claimant => query.Where(claim =>
                claim.ClaimantId == user.Id),

            UserRole.ClaimsOfficer => query.Where(claim =>
                claim.AssignedOfficerId == user.Id ||
                (claim.Status == ClaimStatus.Submitted &&
                 claim.AssignedOfficerId == null &&
                 claim.Market == user.Market)),

            UserRole.Manager when user.TeamId is Guid teamId =>
                query.Where(claim =>
                    claim.Market == user.Market &&
                    (claim.AssignedOfficerId == null ||
                     dbContext.Users.Any(officer =>
                         officer.Id == claim.AssignedOfficerId &&
                         officer.TeamId == teamId))),

            _ => query.Where(_ => false)
        };

        var claim = await query.SingleOrDefaultAsync(cancellationToken);
        return claim?.ToDetailResponse();
    }
}
