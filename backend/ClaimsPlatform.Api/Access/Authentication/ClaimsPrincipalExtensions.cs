using System.Security.Claims;

namespace ClaimsPlatform.Api.Access.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException(
                "The authenticated user ID is missing.");

        return Guid.Parse(value);
    }

    public static string GetMarket(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(
                DemoAuthenticationDefaults.MarketClaimType)?.Value
            ?? throw new InvalidOperationException(
                "The authenticated user market is missing.");
    }

    public static Guid GetTeamId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(
                DemoAuthenticationDefaults.TeamIdClaimType)?.Value
            ?? throw new InvalidOperationException(
                "The authenticated user's team ID is missing.");

        return Guid.Parse(value);
    }
}
