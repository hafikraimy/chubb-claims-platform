using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using ClaimsPlatform.Api.WorkManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsPlatform.Api.WorkManagement.Controllers;

[ApiController]
[Route("api/work")]
[Authorize(Policy = AccessPolicies.ClaimsOfficer)]
public sealed class WorkController(
    WorkService work,
    TeamReportingService reporting)
    : ControllerBase
{
    [HttpGet("queue")]
    public async Task<ActionResult<IReadOnlyCollection<WorkClaimSummaryResponse>>>
        GetQueue(CancellationToken cancellationToken)
    {
        var claims = await work.GetQueueAsync(
            User.GetMarket(),
            cancellationToken);

        return Ok(claims);
    }

    [HttpGet("my-claims")]
    public async Task<ActionResult<IReadOnlyCollection<WorkClaimSummaryResponse>>>
        GetMyClaims(CancellationToken cancellationToken)
    {
        var claims = await work.GetMyClaimsAsync(
            User.GetUserId(),
            cancellationToken);

        return Ok(claims);
    }

    [HttpGet("team-summary")]
    public async Task<ActionResult<TeamSummaryResponse>> GetTeamSummary(
        CancellationToken cancellationToken)
    {
        var summary = await reporting.GetAsync(
            User.GetTeamId(),
            User.GetMarket(),
            DateTimeOffset.UtcNow,
            cancellationToken);

        return Ok(summary);
    }

    [HttpPost("/api/claims/{claimId:guid}/assign-to-me")]
    public async Task<ActionResult<WorkClaimSummaryResponse>> AssignToMe(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        var result = await work.AssignToMeAsync(
            claimId,
            User.GetUserId(),
            User.GetMarket(),
            cancellationToken);

        return result.Outcome switch
        {
            PickupOutcome.Assigned => Ok(result.Claim),
            PickupOutcome.NotFound => NotFound(),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Claim is not available",
                detail: "The claim has already been assigned or progressed.")
        };
    }
}
