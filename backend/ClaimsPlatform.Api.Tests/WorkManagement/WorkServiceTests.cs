using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Tests.TestSupport;
using ClaimsPlatform.Api.WorkManagement.Services;

namespace ClaimsPlatform.Api.Tests.WorkManagement;

public class WorkServiceTests
{
    [Fact]
    public async Task GetQueueAsync_ReturnsOnlySubmittedUnassignedMarketClaimsOldestFirst()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var oldest = TestData.Claim(submittedAt: TestData.Now.AddDays(-3));
        var newest = TestData.Claim(submittedAt: TestData.Now.AddDays(-1));
        var assigned = TestData.Claim(submittedAt: TestData.Now.AddDays(-4));
        assigned.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now);
        var otherMarket = TestData.Claim(market: "SG");
        db.Claims.AddRange(newest, assigned, otherMarket, oldest);
        await db.SaveChangesAsync();

        var result = await new WorkService(db).GetQueueAsync("MY", default);

        Assert.Equal([oldest.Id, newest.Id], result.Select(claim => claim.Id));
    }

    [Fact]
    public async Task GetMyClaimsAsync_ReturnsOnlyOfficersOpenClaimsNewestFirst()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var older = TestData.Claim(submittedAt: TestData.Now.AddDays(-3));
        older.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddHours(-2));
        var newer = TestData.Claim(submittedAt: TestData.Now.AddDays(-2));
        newer.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddHours(-1));
        var terminal = TestData.Claim();
        terminal.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddHours(-2));
        terminal.Reject(TestData.OfficerId, "Not covered", TestData.Now);
        var anotherOfficer = TestData.Claim();
        anotherOfficer.AssignTo(TestData.OtherOfficerId, TestData.OtherOfficerId, TestData.Now);
        db.Claims.AddRange(older, newer, terminal, anotherOfficer);
        await db.SaveChangesAsync();

        var result = await new WorkService(db).GetMyClaimsAsync(TestData.OfficerId, default);

        Assert.Equal([newer.Id, older.Id], result.Select(claim => claim.Id));
    }

    [Fact]
    public async Task AssignToMeAsync_EligibleClaim_AssignsAndReturnsSummary()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var claim = TestData.Claim();
        db.Claims.Add(claim);
        await db.SaveChangesAsync();

        var result = await new WorkService(db).AssignToMeAsync(
            claim.Id,
            TestData.OfficerId,
            "MY",
            default);

        Assert.Equal(PickupOutcome.Assigned, result.Outcome);
        Assert.Equal(TestData.OfficerId, result.Claim!.AssignedOfficerId);
        Assert.Equal(ClaimStatus.InReview, result.Claim.Status);
        Assert.Contains(
            db.ClaimHistoryEntries,
            entry => entry.ClaimId == claim.Id &&
                     entry.EventType == ClaimHistoryEventType.Assigned);
    }

    [Fact]
    public async Task AssignToMeAsync_AlreadyAssignedClaim_ReturnsUnavailable()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var claim = TestData.Claim();
        claim.AssignTo(TestData.OtherOfficerId, TestData.OtherOfficerId, TestData.Now);
        db.Claims.Add(claim);
        await db.SaveChangesAsync();

        var result = await new WorkService(db).AssignToMeAsync(
            claim.Id,
            TestData.OfficerId,
            "MY",
            default);

        Assert.Equal(PickupOutcome.Unavailable, result.Outcome);
        Assert.Null(result.Claim);
        Assert.Equal(TestData.OtherOfficerId, claim.AssignedOfficerId);
    }

    [Fact]
    public async Task AssignToMeAsync_OutsideMarket_ReturnsNotFound()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var claim = TestData.Claim(market: "SG");
        db.Claims.Add(claim);
        await db.SaveChangesAsync();

        var result = await new WorkService(db).AssignToMeAsync(
            claim.Id,
            TestData.OfficerId,
            "MY",
            default);

        Assert.Equal(PickupOutcome.NotFound, result.Outcome);
        Assert.Null(result.Claim);
    }
}
