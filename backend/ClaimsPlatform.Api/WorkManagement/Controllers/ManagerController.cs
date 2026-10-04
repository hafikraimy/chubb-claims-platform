using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using ClaimsPlatform.Api.WorkManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsPlatform.Api.WorkManagement.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Policy = AccessPolicies.Manager)]
public sealed class ManagerController(ManagerWorkService managerWork)
    : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<ManagerDashboardResponse>> GetDashboard(
        CancellationToken cancellationToken)
    {
        var dashboard = await managerWork.GetDashboardAsync(
            User.GetTeamId(),
            User.GetMarket(),
            cancellationToken);

        return Ok(dashboard);
    }

    [HttpPost("/api/claims/{claimId:guid}/assignments")]
    public async Task<ActionResult<ClaimAssignmentResponse>> Assign(
        Guid claimId,
        AssignClaimRequest request,
        CancellationToken cancellationToken)
    {
        var result = await managerWork.AssignAsync(
            claimId,
            request.OfficerId,
            User.GetUserId(),
            User.GetTeamId(),
            User.GetMarket(),
            cancellationToken);

        return result.Outcome switch
        {
            ManagerAssignmentOutcome.Assigned => Ok(result.Assignment),
            ManagerAssignmentOutcome.NotFound => NotFound(),
            _ => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid officer",
                detail: "The selected officer is not a member of your team.")
        };
    }
}
