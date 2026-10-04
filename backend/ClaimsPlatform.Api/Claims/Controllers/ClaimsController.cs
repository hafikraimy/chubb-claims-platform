using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Claims.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsPlatform.Api.Claims.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize]
public sealed class ClaimsController(
    ClaimantClaimService claimantClaims,
    ClaimQueryService claimQueries)
    : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AccessPolicies.Claimant)]
    public async Task<ActionResult<ClaimCreatedResponse>> Submit(
        SubmitClaimRequest request,
        CancellationToken cancellationToken)
    {
        var user = User.GetDemoUser();
        var response = await claimantClaims.SubmitAsync(
            request,
            user.Id,
            user.Market,
            cancellationToken);

        return Created($"/api/claims/{response.Id}", response);
    }

    [HttpGet]
    [Authorize(Policy = AccessPolicies.Claimant)]
    public async Task<ActionResult<IReadOnlyCollection<ClaimSummaryResponse>>>
        GetClaims(CancellationToken cancellationToken)
    {
        var claims = await claimantClaims.GetClaimsAsync(
            User.GetUserId(),
            cancellationToken);

        return Ok(claims);
    }

    [HttpGet("{claimId:guid}")]
    public async Task<ActionResult<ClaimDetailResponse>> GetClaim(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        var claim = await claimQueries.GetAsync(
            claimId,
            User.GetDemoUser(),
            cancellationToken);

        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpPost(
        "{claimId:guid}/information-requests/{requestId:guid}/response")]
    [Authorize(Policy = AccessPolicies.Claimant)]
    public async Task<ActionResult<ClaimUpdatedResponse>> Respond(
        Guid claimId,
        Guid requestId,
        RespondToInformationRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await claimantClaims.RespondAsync(
            claimId,
            requestId,
            request.Response,
            User.GetUserId(),
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }
}
