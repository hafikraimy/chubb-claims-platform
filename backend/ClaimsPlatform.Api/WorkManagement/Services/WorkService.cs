using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using Microsoft.EntityFrameworkCore;
using DomainClaim = ClaimsPlatform.Api.Claims.Domain.Claim;

namespace ClaimsPlatform.Api.WorkManagement.Services;

public sealed class WorkService(ClaimsDbContext dbContext)
{
    public Task<List<WorkClaimSummaryResponse>> GetQueueAsync(
        string market,
        CancellationToken cancellationToken)
    {
        return dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.Status == ClaimStatus.Submitted &&
                claim.AssignedOfficerId == null &&
                claim.Market == market)
            .OrderBy(claim => claim.SubmittedAt)
            .Select(claim => ToSummary(claim))
            .ToListAsync(cancellationToken);
    }

    public Task<List<WorkClaimSummaryResponse>> GetMyClaimsAsync(
        Guid officerId,
        CancellationToken cancellationToken)
    {
        return dbContext.Claims
            .AsNoTracking()
            .Where(claim =>
                claim.AssignedOfficerId == officerId &&
                claim.Status != ClaimStatus.Settled &&
                claim.Status != ClaimStatus.Rejected)
            .OrderByDescending(claim => claim.UpdatedAt)
            .Select(claim => ToSummary(claim))
            .ToListAsync(cancellationToken);
    }

    public async Task<PickupResult> AssignToMeAsync(
        Guid claimId,
        Guid officerId,
        string market,
        CancellationToken cancellationToken)
    {
        var claim = await dbContext.Claims.SingleOrDefaultAsync(
            candidate =>
                candidate.Id == claimId &&
                candidate.Market == market,
            cancellationToken);

        if (claim is null)
        {
            return new PickupResult(PickupOutcome.NotFound, null);
        }

        if (claim.Status != ClaimStatus.Submitted ||
            claim.AssignedOfficerId is not null)
        {
            return new PickupResult(PickupOutcome.Unavailable, null);
        }

        claim.AssignTo(officerId, officerId, DateTimeOffset.UtcNow);
        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PickupResult(
            PickupOutcome.Assigned,
            ToSummary(claim));
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

public enum PickupOutcome
{
    Assigned,
    NotFound,
    Unavailable
}

public sealed record PickupResult(
    PickupOutcome Outcome,
    WorkClaimSummaryResponse? Claim);
