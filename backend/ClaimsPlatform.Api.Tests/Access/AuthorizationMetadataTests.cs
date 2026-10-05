using System.Reflection;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Claims.Controllers;
using ClaimsPlatform.Api.WorkManagement.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace ClaimsPlatform.Api.Tests.Access;

public class AuthorizationMetadataTests
{
    [Theory]
    [InlineData(typeof(ClaimWorkflowController), AccessPolicies.ClaimsOfficer)]
    [InlineData(typeof(WorkController), AccessPolicies.ClaimsOfficer)]
    [InlineData(typeof(ManagerController), AccessPolicies.Manager)]
    public void RoleSpecificController_RequiresExpectedPolicy(
        Type controllerType,
        string policy)
    {
        var attribute = Assert.Single(
            controllerType.GetCustomAttributes<AuthorizeAttribute>());

        Assert.Equal(policy, attribute.Policy);
    }

    [Theory]
    [InlineData(nameof(ClaimsController.Submit))]
    [InlineData(nameof(ClaimsController.GetClaims))]
    [InlineData(nameof(ClaimsController.Respond))]
    public void ClaimantCommand_RequiresClaimantPolicy(string methodName)
    {
        var method = typeof(ClaimsController).GetMethod(methodName);
        var attribute = Assert.Single(
            method!.GetCustomAttributes<AuthorizeAttribute>());

        Assert.Equal(AccessPolicies.Claimant, attribute.Policy);
    }

    [Fact]
    public void ClaimDetail_RequiresAuthenticationWithoutRestrictingOneRole()
    {
        var controllerAuthorization = Assert.Single(
            typeof(ClaimsController).GetCustomAttributes<AuthorizeAttribute>());
        var method = typeof(ClaimsController).GetMethod(nameof(ClaimsController.GetClaim));

        Assert.Null(controllerAuthorization.Policy);
        Assert.Empty(method!.GetCustomAttributes<AuthorizeAttribute>());
    }
}
