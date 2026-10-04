using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Claims.Services;

public sealed class ClaimWorkflowService(ClaimsDbContext dbContext)
{
    public async Task<ClaimUpdatedResponse?> RecordAssessedLossAsync(
        Guid claimId,
        Guid officerId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var claim = await FindAssignedClaimAsync(
            claimId, officerId, cancellationToken);

        if (claim is null)
        {
            return null;
        }

        claim.RecordAssessedLoss(officerId, amount, DateTimeOffset.UtcNow);
        await SaveAsync(claim, cancellationToken);
        return ToUpdatedResponse(claim);
    }

    public async Task<InformationRequestCreatedResponse?> RequestInformationAsync(
        Guid claimId,
        Guid officerId,
        string question,
        CancellationToken cancellationToken)
    {
        var claim = await FindAssignedClaimAsync(
            claimId, officerId, cancellationToken);

        if (claim is null)
        {
            return null;
        }

        var requestedAt = DateTimeOffset.UtcNow;
        var requestId = claim.RequestInformation(
            officerId, question, requestedAt);

        dbContext.InformationRequests.Add(
            claim.InformationRequests.Single(request =>
                request.Id == requestId));
        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
        await dbContext.SaveChangesAsync(cancellationToken);

        return new InformationRequestCreatedResponse(
            requestId,
            claim.Status,
            requestedAt);
    }

    public async Task<ClaimUpdatedResponse?> SettleAsync(
        Guid claimId,
        Guid officerId,
        SettleClaimRequest request,
        CancellationToken cancellationToken)
    {
        var claim = await FindAssignedClaimAsync(
            claimId, officerId, cancellationToken);

        if (claim is null)
        {
            return null;
        }

        claim.Settle(
            officerId,
            request.SettlementAmount,
            request.Reason,
            DateTimeOffset.UtcNow);
        await SaveAsync(claim, cancellationToken);
        return ToUpdatedResponse(claim);
    }

    public async Task<ClaimUpdatedResponse?> RejectAsync(
        Guid claimId,
        Guid officerId,
        string reason,
        CancellationToken cancellationToken)
    {
        var claim = await FindAssignedClaimAsync(
            claimId, officerId, cancellationToken);

        if (claim is null)
        {
            return null;
        }

        claim.Reject(officerId, reason, DateTimeOffset.UtcNow);
        await SaveAsync(claim, cancellationToken);
        return ToUpdatedResponse(claim);
    }

    private Task<Domain.Claim?> FindAssignedClaimAsync(
        Guid claimId,
        Guid officerId,
        CancellationToken cancellationToken)
    {
        return dbContext.Claims.SingleOrDefaultAsync(
            claim =>
                claim.Id == claimId &&
                claim.AssignedOfficerId == officerId,
            cancellationToken);
    }

    private async Task SaveAsync(
        Domain.Claim claim,
        CancellationToken cancellationToken)
    {
        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ClaimUpdatedResponse ToUpdatedResponse(
        Domain.Claim claim)
    {
        return new ClaimUpdatedResponse(
            claim.Id,
            claim.Status,
            claim.UpdatedAt);
    }
}
