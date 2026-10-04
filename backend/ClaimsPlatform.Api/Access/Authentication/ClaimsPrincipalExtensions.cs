using System.Security.Claims;
using ClaimsPlatform.Api.Access.Domain;

namespace ClaimsPlatform.Api.Access.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static DemoUserContext GetDemoUser(this ClaimsPrincipal principal)
    {
        var roleValue = principal.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new InvalidOperationException(
                "The authenticated user role is missing.");

        var teamIdValue = principal.FindFirst(
            DemoAuthenticationDefaults.TeamIdClaimType)?.Value;

        return new DemoUserContext(
            principal.GetUserId(),
            Enum.Parse<UserRole>(roleValue),
            principal.GetMarket(),
            teamIdValue is null ? null : Guid.Parse(teamIdValue));
    }

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

public sealed record DemoUserContext(
    Guid Id,
    UserRole Role,
    string Market,
    Guid? TeamId);
