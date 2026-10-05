using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Claims.Services;
using ClaimsPlatform.Api.Tests.TestSupport;

namespace ClaimsPlatform.Api.Tests.Claims.Services;

public class ClaimQueryServiceTests
{
    [Fact]
    public async Task GetAsync_Claimant_CanOnlyReadOwnedClaim()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var own = TestData.Claim();
        var other = TestData.Claim(TestData.OtherClaimantId);
        db.Claims.AddRange(own, other);
        await db.SaveChangesAsync();
        var service = new ClaimQueryService(db);
        var user = new DemoUserContext(TestData.ClaimantId, UserRole.Claimant, "MY", null);

        Assert.NotNull(await service.GetAsync(own.Id, user, default));
        Assert.Null(await service.GetAsync(other.Id, user, default));
    }

    [Fact]
    public async Task GetAsync_Officer_CanReadAssignedAndEligibleQueueClaimsOnly()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var assigned = TestData.Claim();
        assigned.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now);
        var queue = TestData.Claim();
        var otherMarket = TestData.Claim(market: "SG");
        db.Claims.AddRange(assigned, queue, otherMarket);
        await db.SaveChangesAsync();
        var service = new ClaimQueryService(db);
        var user = new DemoUserContext(
            TestData.OfficerId,
            UserRole.ClaimsOfficer,
            "MY",
            TestData.TeamId);

        Assert.NotNull(await service.GetAsync(assigned.Id, user, default));
        Assert.NotNull(await service.GetAsync(queue.Id, user, default));
        Assert.Null(await service.GetAsync(otherMarket.Id, user, default));
    }

    [Fact]
    public async Task GetAsync_Manager_CannotReadClaimAssignedOutsideTeam()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var outsideTeam = TestData.Claim(market: "SG");
        outsideTeam.AssignTo(
            TestData.OtherMarketOfficerId,
            TestData.OtherMarketOfficerId,
            TestData.Now);
        db.Claims.Add(outsideTeam);
        await db.SaveChangesAsync();
        var service = new ClaimQueryService(db);
        var manager = new DemoUserContext(
            TestData.ManagerId,
            UserRole.Manager,
            "MY",
            TestData.TeamId);

        Assert.Null(await service.GetAsync(outsideTeam.Id, manager, default));
    }
}
