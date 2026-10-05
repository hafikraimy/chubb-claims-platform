using System.Security.Claims;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Tests.TestSupport;
using ClaimsPlatform.Api.WorkManagement.Controllers;
using ClaimsPlatform.Api.WorkManagement.Dtos;
using ClaimsPlatform.Api.WorkManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsPlatform.Api.Tests.WorkManagement;

public class ControllerContractTests
{
    [Fact]
    public async Task Pickup_AlreadyAssignedClaim_ReturnsConflictProblem()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var claim = TestData.Claim();
        claim.AssignTo(TestData.OtherOfficerId, TestData.OtherOfficerId, TestData.Now);
        db.Claims.Add(claim);
        await db.SaveChangesAsync();
        var controller = new WorkController(
            new WorkService(db),
            new TeamReportingService(db));
        SetUser(controller, TestData.OfficerId, UserRole.ClaimsOfficer);

        var response = await controller.AssignToMe(claim.Id, default);

        var result = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Claim is not available", problem.Title);
    }

    [Fact]
    public async Task ManagerAssignment_OfficerOutsideTeam_ReturnsBadRequestProblem()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var claim = TestData.Claim();
        db.Claims.Add(claim);
        await db.SaveChangesAsync();
        var controller = new ManagerController(
            new ManagerWorkService(db, new TeamReportingService(db)));
        SetUser(controller, TestData.ManagerId, UserRole.Manager);

        var response = await controller.Assign(
            claim.Id,
            new AssignClaimRequest(TestData.OtherMarketOfficerId),
            default);

        var result = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Invalid officer", problem.Title);
    }

    private static void SetUser(
        ControllerBase controller,
        Guid userId,
        UserRole role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role.ToString()),
            new(DemoAuthenticationDefaults.MarketClaimType, "MY"),
            new(DemoAuthenticationDefaults.TeamIdClaimType, TestData.TeamId.ToString())
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }
}
