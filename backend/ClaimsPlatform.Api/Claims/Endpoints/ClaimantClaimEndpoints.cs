using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainClaim = ClaimsPlatform.Api.Claims.Domain.Claim;

namespace ClaimsPlatform.Api.Claims.Endpoints;

public static class ClaimantClaimEndpoints
{
    public static IEndpointRouteBuilder MapClaimantClaimEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/claims")
            .RequireAuthorization(AccessPolicies.Claimant);

        group.MapPost("/", SubmitClaimAsync);
        group.MapGet("/", GetClaimsAsync);
        group.MapPost(
            "/{claimId:guid}/information-requests/{requestId:guid}/response",
            RespondToInformationRequestAsync);

        return endpoints;
    }

    private static async Task<IResult> SubmitClaimAsync(
        SubmitClaimRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var claim = DomainClaim.Submit(
            claimantId: principal.GetUserId(),
            type: request.Type,
            policyNumber: request.PolicyNumber,
            market: principal.GetMarket(),
            currency: request.Currency,
            incidentDate: request.IncidentDate,
            incidentLocation: request.IncidentLocation,
            description: request.Description,
            reportedLossAmount: request.ReportedLossAmount,
            submittedAt: DateTimeOffset.UtcNow);

        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/claims/{claim.Id}",
            new ClaimCreatedResponse(
                claim.Id,
                claim.ReferenceNumber,
                claim.Status));
    }

    private static async Task<IResult> GetClaimsAsync(
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var claimantId = principal.GetUserId();

        var claims = await dbContext.Claims
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

        return Results.Ok(claims);
    }

    private static async Task<IResult> RespondToInformationRequestAsync(
        Guid claimId,
        Guid requestId,
        RespondToInformationRequestRequest request,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var claimantId = principal.GetUserId();

        var claim = await dbContext.Claims
            .Include(candidate => candidate.InformationRequests)
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.Id == claimId &&
                    candidate.ClaimantId == claimantId,
                cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        claim.RespondToInformationRequest(
            claimantId,
            requestId,
            request.Response,
            DateTimeOffset.UtcNow);

        dbContext.ClaimHistoryEntries.Add(claim.History.Last());

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new ClaimUpdatedResponse(
            claim.Id,
            claim.Status,
            claim.UpdatedAt));
    }
}
