using System.Security.Claims;
using System.Text.Encodings.Web;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClaimsPlatform.Api.Access.Authentication;

public class DemoAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ClaimsDbContext dbContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options,
        logger,
        encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                DemoAuthenticationDefaults.HeaderName,
                out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        if (!Guid.TryParse(headerValue.ToString(), out var userId))
        {
            return AuthenticateResult.Fail(
                "The X-Demo-User header must contain a valid user ID.");
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == userId,
                Context.RequestAborted);

        if (user is null)
        {
            return AuthenticateResult.Fail("The demo user was not found.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(DemoAuthenticationDefaults.MarketClaimType, user.Market)
        };

        if (user.TeamId is Guid teamId)
        {
            claims.Add(new Claim(
                DemoAuthenticationDefaults.TeamIdClaimType,
                teamId.ToString()));
        }

        var identity = new ClaimsIdentity(
            claims,
            DemoAuthenticationDefaults.Scheme);

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
