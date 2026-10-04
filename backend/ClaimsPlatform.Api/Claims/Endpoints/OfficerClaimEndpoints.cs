using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Claims.Endpoints;

public static class OfficerClaimEndpoints
{
    public static IEndpointRouteBuilder MapOfficerClaimEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var commandGroup = endpoints.MapGroup("/api/claims/{claimId:guid}")
            .RequireAuthorization(AccessPolicies.ClaimsOfficer);

        commandGroup.MapPost("/assessed-loss", RecordAssessedLossAsync);
        commandGroup.MapPost(
            "/information-requests",
            RequestInformationAsync);
        commandGroup.MapPost("/settle", SettleClaimAsync);
        commandGroup.MapPost("/reject", RejectClaimAsync);

        return endpoints;
    }

    private static async Task<IResult> RecordAssessedLossAsync(
        Guid claimId,
        RecordAssessedLossRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var claim = await FindAssignedClaimAsync(
            claimId,
            principal.GetUserId(),
            dbContext,
            cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        claim.RecordAssessedLoss(
            principal.GetUserId(),
            request.Amount,
            DateTimeOffset.UtcNow);

        AddLatestHistory(dbContext, claim);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToUpdatedResponse(claim));
    }

    private static async Task<IResult> RequestInformationAsync(
        Guid claimId,
        CreateInformationRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var officerId = principal.GetUserId();

        var claim = await FindAssignedClaimAsync(
            claimId,
            officerId,
            dbContext,
            cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        var requestedAt = DateTimeOffset.UtcNow;

        var requestId = claim.RequestInformation(
            officerId,
            request.Question,
            requestedAt);

        var informationRequest = claim.InformationRequests
            .Single(candidate => candidate.Id == requestId);

        dbContext.InformationRequests.Add(informationRequest);
        AddLatestHistory(dbContext, claim);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new InformationRequestCreatedResponse(
            requestId,
            claim.Status,
            requestedAt));
    }

    private static async Task<IResult> SettleClaimAsync(
        Guid claimId,
        SettleClaimRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var officerId = principal.GetUserId();

        var claim = await FindAssignedClaimAsync(
            claimId,
            officerId,
            dbContext,
            cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        claim.Settle(
            officerId,
            request.SettlementAmount,
            request.Reason,
            DateTimeOffset.UtcNow);

        AddLatestHistory(dbContext, claim);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToUpdatedResponse(claim));
    }

    private static async Task<IResult> RejectClaimAsync(
        Guid claimId,
        RejectClaimRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var officerId = principal.GetUserId();

        var claim = await FindAssignedClaimAsync(
            claimId,
            officerId,
            dbContext,
            cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        claim.Reject(
            officerId,
            request.Reason,
            DateTimeOffset.UtcNow);

        AddLatestHistory(dbContext, claim);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToUpdatedResponse(claim));
    }

    private static Task<Domain.Claim?> FindAssignedClaimAsync(
        Guid claimId,
        Guid officerId,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.Claims.SingleOrDefaultAsync(
            candidate =>
                candidate.Id == claimId &&
                candidate.AssignedOfficerId == officerId,
            cancellationToken);
    }

    private static void AddLatestHistory(
        ClaimsDbContext dbContext,
        Domain.Claim claim)
    {
        dbContext.ClaimHistoryEntries.Add(claim.History.Last());
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
