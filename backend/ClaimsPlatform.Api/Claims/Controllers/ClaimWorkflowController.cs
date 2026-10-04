using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Claims.Dtos;
using ClaimsPlatform.Api.Claims.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsPlatform.Api.Claims.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}")]
[Authorize(Policy = AccessPolicies.ClaimsOfficer)]
public sealed class ClaimWorkflowController(
    ClaimWorkflowService workflow)
    : ControllerBase
{
    [HttpPost("assessed-loss")]
    public async Task<ActionResult<ClaimUpdatedResponse>> RecordAssessedLoss(
        Guid claimId,
        RecordAssessedLossRequest request,
        CancellationToken cancellationToken)
    {
        var response = await workflow.RecordAssessedLossAsync(
            claimId,
            User.GetUserId(),
            request.Amount,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("information-requests")]
    public async Task<ActionResult<InformationRequestCreatedResponse>>
        RequestInformation(
            Guid claimId,
            CreateInformationRequest request,
            CancellationToken cancellationToken)
    {
        var response = await workflow.RequestInformationAsync(
            claimId,
            User.GetUserId(),
            request.Question,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("settle")]
    public async Task<ActionResult<ClaimUpdatedResponse>> Settle(
        Guid claimId,
        SettleClaimRequest request,
        CancellationToken cancellationToken)
    {
        var response = await workflow.SettleAsync(
            claimId,
            User.GetUserId(),
            request,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("reject")]
    public async Task<ActionResult<ClaimUpdatedResponse>> Reject(
        Guid claimId,
        RejectClaimRequest request,
        CancellationToken cancellationToken)
    {
        var response = await workflow.RejectAsync(
            claimId,
            User.GetUserId(),
            request.Reason,
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }
}
