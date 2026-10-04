using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Claims.Services;

public sealed class ClaimantClaimService(ClaimsDbContext dbContext)
{
    public async Task<ClaimCreatedResponse> SubmitAsync(
        SubmitClaimRequest request,
        Guid claimantId,
        string market,
        CancellationToken cancellationToken)
    {
        var claim = Claim.Submit(
            claimantId,
            request.Type,
            request.PolicyNumber,
            market,
            request.Currency,
            request.IncidentDate,
            request.IncidentLocation,
            request.Description,
            request.ReportedLossAmount,
            DateTimeOffset.UtcNow);

        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClaimCreatedResponse(
            claim.Id,
            claim.ReferenceNumber,
            claim.Status);
    }

    public Task<List<ClaimSummaryResponse>> GetClaimsAsync(
        Guid claimantId,
        CancellationToken cancellationToken)
    {
        return dbContext.Claims
            .AsNoTracking()
            .Where(claim => claim.ClaimantId == claimantId)
            .OrderByDescending(claim => claim.SubmittedAt)
            .Select(claim => new ClaimSummaryResponse(
                claim.Id,
                claim.ReferenceNumber,
                claim.Type,
                claim.IncidentDate,
                claim.ReportedLossAmount,
                claim.Currency,
                claim.Status,
                claim.SubmittedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClaimUpdatedResponse?> RespondAsync(
        Guid claimId,
        Guid requestId,
        string response,
        Guid claimantId,
        CancellationToken cancellationToken)
    {
        var claim = await dbContext.Claims
            .Include(candidate => candidate.InformationRequests)
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.Id == claimId &&
                    candidate.ClaimantId == claimantId,
                cancellationToken);

        if (claim is null)
        {
            return null;
        }

        claim.RespondToInformationRequest(
            claimantId,
            requestId,
            response,
            DateTimeOffset.UtcNow);

        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClaimUpdatedResponse(
            claim.Id,
            claim.Status,
            claim.UpdatedAt);
    }
}
