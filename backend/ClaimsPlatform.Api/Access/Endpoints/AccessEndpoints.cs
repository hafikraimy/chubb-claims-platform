using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;

namespace ClaimsPlatform.Api.Access.Endpoints;

public static class AccessEndpoints
{
    public static IEndpointRouteBuilder MapAccessEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/me", (ClaimsPrincipal principal) =>
        {
            var id = Guid.Parse(
                principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var name = principal.FindFirst(ClaimTypes.Name)!.Value;
            var role = principal.FindFirst(ClaimTypes.Role)!.Value;
            var market = principal.FindFirst(
                DemoAuthenticationDefaults.MarketClaimType)!.Value;

            var teamIdValue = principal.FindFirst(
                DemoAuthenticationDefaults.TeamIdClaimType)?.Value;

            var teamId = teamIdValue is null
                ? (Guid?)null
                : Guid.Parse(teamIdValue);

            return Results.Ok(new CurrentUserResponse(
                id,
                name,
                role,
                market,
                teamId));
        })
        .RequireAuthorization();

        return endpoints;
    }

    private sealed record CurrentUserResponse(
        Guid Id,
        string Name,
        string Role,
        string Market,
        Guid? TeamId);
}
