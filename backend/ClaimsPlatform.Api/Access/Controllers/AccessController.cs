using ClaimsPlatform.Api.Access.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsPlatform.Api.Access.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class AccessController : ControllerBase
{
    [HttpGet]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        var user = User.GetDemoUser();

        return Ok(new CurrentUserResponse(
            user.Id,
            User.Identity!.Name!,
            user.Role.ToString(),
            user.Market,
            user.TeamId));
    }
}

public sealed record CurrentUserResponse(
    Guid Id,
    string Name,
    string Role,
    string Market,
    Guid? TeamId);
