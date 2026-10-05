using ClaimsPlatform.Api.Tests.TestSupport;
using ClaimsPlatform.Api.WorkManagement.Services;

namespace ClaimsPlatform.Api.Tests.WorkManagement;

public class ReportingServiceTests
{
    [Fact]
    public async Task TeamSummary_CalculatesWorkloadAndThirtyDayPerformance()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var inReview = TestData.Claim(submittedAt: TestData.Now.AddDays(-4));
        inReview.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddDays(-3));
        var awaiting = TestData.Claim(submittedAt: TestData.Now.AddDays(-2));
        awaiting.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddDays(-2));
        awaiting.RequestInformation(TestData.OfficerId, "Police report?", TestData.Now.AddDays(-1));
        var settled = TestData.Claim(submittedAt: TestData.Now.AddDays(-2));
        settled.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddDays(-2));
        settled.Settle(TestData.OfficerId, 900m, "Approved", TestData.Now);
        var oldDecision = TestData.Claim(submittedAt: TestData.Now.AddDays(-40));
        oldDecision.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddDays(-40));
        oldDecision.Reject(TestData.OfficerId, "Not covered", TestData.Now.AddDays(-31));
        db.Claims.AddRange(inReview, awaiting, settled, oldDecision);
        await db.SaveChangesAsync();

        var summary = await new TeamReportingService(db).GetAsync(
            TestData.TeamId,
            "MY",
            TestData.Now,
            default);

        var ben = Assert.Single(summary.Workload, row => row.OfficerId == TestData.OfficerId);
        Assert.Equal(2, ben.OpenClaimCount);
        Assert.Equal(1, ben.InReviewCount);
        Assert.Equal(1, ben.AwaitingInfoCount);
        Assert.Equal(3m, ben.AverageOpenClaimAgeDays);
        Assert.Equal(4m, ben.OldestOpenClaimAgeDays);
        var performance = Assert.Single(
            summary.Performance.Officers,
            row => row.OfficerId == TestData.OfficerId);
        Assert.Equal(1, performance.SettledCount);
        Assert.Equal(0, performance.RejectedCount);
        Assert.Equal(1, performance.TotalDecisions);
        Assert.Equal(48m, performance.AverageDecisionHours);
    }

    [Fact]
    public async Task ManagerDashboard_UsesAssessedFallbackAndGroupsExposureByCurrency()
    {
        await using var db = TestData.CreateContext();
        await TestData.SeedUsersAsync(db);
        var reportedFallback = TestData.Claim(loss: 1_000m);
        var assessed = TestData.Claim(loss: 2_000m);
        assessed.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddDays(-1));
        assessed.RecordAssessedLoss(TestData.OfficerId, 1_500m, TestData.Now);
        var usd = TestData.Claim(currency: "USD", loss: 300m);
        var settled = TestData.Claim(loss: 9_000m);
        settled.AssignTo(TestData.OfficerId, TestData.OfficerId, TestData.Now.AddDays(-1));
        settled.Settle(TestData.OfficerId, 8_000m, "Approved", TestData.Now);
        db.Claims.AddRange(reportedFallback, assessed, usd, settled);
        await db.SaveChangesAsync();

        var dashboard = await new ManagerWorkService(
            db,
            new TeamReportingService(db)).GetDashboardAsync(
                TestData.TeamId,
                "MY",
                default);

        Assert.Equal(3, dashboard.Claims.Count);
        Assert.Equal(2_500m, dashboard.Exposure.Single(x => x.Currency == "MYR").Amount);
        Assert.Equal(300m, dashboard.Exposure.Single(x => x.Currency == "USD").Amount);
        Assert.DoesNotContain(dashboard.Claims, claim => claim.Id == settled.Id);
    }
}
