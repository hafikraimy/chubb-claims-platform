using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
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
