using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Tests.TestSupport;

namespace ClaimsPlatform.Api.Tests.Access;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetDemoUser_WithCompleteOfficerIdentity_ReturnsContext()
    {
        var principal = Principal(
            new(ClaimTypes.NameIdentifier, TestData.OfficerId.ToString()),
            new(ClaimTypes.Role, UserRole.ClaimsOfficer.ToString()),
            new(DemoAuthenticationDefaults.MarketClaimType, "MY"),
            new(DemoAuthenticationDefaults.TeamIdClaimType, TestData.TeamId.ToString()));

        var user = principal.GetDemoUser();

        Assert.Equal(TestData.OfficerId, user.Id);
        Assert.Equal(UserRole.ClaimsOfficer, user.Role);
        Assert.Equal("MY", user.Market);
        Assert.Equal(TestData.TeamId, user.TeamId);
    }

    [Fact]
    public void GetDemoUser_ClaimantWithoutTeam_ReturnsContext()
    {
        var principal = Principal(
            new(ClaimTypes.NameIdentifier, TestData.ClaimantId.ToString()),
            new(ClaimTypes.Role, UserRole.Claimant.ToString()),
            new(DemoAuthenticationDefaults.MarketClaimType, "MY"));

        Assert.Null(principal.GetDemoUser().TeamId);
    }

    [Theory]
    [InlineData(ClaimTypes.NameIdentifier, "The authenticated user ID is missing.")]
    [InlineData(ClaimTypes.Role, "The authenticated user role is missing.")]
    [InlineData(DemoAuthenticationDefaults.MarketClaimType, "The authenticated user market is missing.")]
    public void GetDemoUser_WithMissingRequiredClaim_Throws(
        string omittedType,
        string expectedMessage)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestData.OfficerId.ToString()),
            new Claim(ClaimTypes.Role, UserRole.ClaimsOfficer.ToString()),
            new Claim(DemoAuthenticationDefaults.MarketClaimType, "MY")
        }.Where(claim => claim.Type != omittedType).ToArray();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Principal(claims).GetDemoUser());

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void GetTeamId_WithoutTeamClaim_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Principal().GetTeamId());

        Assert.Equal("The authenticated user's team ID is missing.", exception.Message);
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Test"));
}
