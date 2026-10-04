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
        group.MapGet("/{claimId:guid}", GetClaimAsync);

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

    private static async Task<IResult> GetClaimAsync(
        Guid claimId,
        ClaimsPrincipal principal,
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var claimantId = principal.GetUserId();

        var claim = await dbContext.Claims
            .AsNoTracking()
            .AsSplitQuery()
            .Include(candidate => candidate.InformationRequests)
            .Include(candidate => candidate.History)
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.Id == claimId &&
                    candidate.ClaimantId == claimantId,
                cancellationToken);

        if (claim is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToDetailResponse(claim));
    }

    private static ClaimDetailResponse ToDetailResponse(DomainClaim claim)
    {
        var informationRequests = claim.InformationRequests
            .OrderByDescending(request => request.RequestedAt)
            .Select(request => new InformationRequestResponse(
                request.Id,
                request.RequestedByOfficerId,
                request.Question,
                request.RequestedAt,
                request.Response,
                request.RespondedAt))
            .ToArray();

        var history = claim.History
            .OrderBy(entry => entry.OccurredAt)
            .Select(entry => new ClaimHistoryResponse(
                entry.Id,
                entry.ActingUserId,
                entry.EventType,
                entry.Description,
                entry.OccurredAt))
            .ToArray();

        return new ClaimDetailResponse(
            claim.Id,
            claim.ReferenceNumber,
            claim.Type,
            claim.PolicyNumber,
            claim.Market,
            claim.Currency,
            claim.IncidentDate,
            claim.IncidentLocation,
            claim.Description,
            claim.ReportedLossAmount,
            claim.AssessedLossAmount,
            claim.Status,
            claim.AssignedOfficerId,
            claim.SubmittedAt,
            claim.UpdatedAt,
            claim.DecisionReason,
            claim.SettlementAmount,
            informationRequests,
            history);
    }
}
