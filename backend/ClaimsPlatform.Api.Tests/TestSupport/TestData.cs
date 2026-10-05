using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Tests.TestSupport;

internal static class TestData
{
    internal static readonly Guid ClaimantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    internal static readonly Guid OtherClaimantId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    internal static readonly Guid OfficerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    internal static readonly Guid OtherOfficerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    internal static readonly Guid OtherMarketOfficerId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    internal static readonly Guid ManagerId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    internal static readonly Guid TeamId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    internal static readonly Guid OtherTeamId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    internal static readonly DateTimeOffset Now = new(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);

    internal static ClaimsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClaimsDbContext(options);
    }

    internal static async Task SeedUsersAsync(ClaimsDbContext db)
    {
        var team = Team.Create(TeamId, "Malaysia Claims", "MY");
        team.AssignManager(ManagerId);
        db.Teams.AddRange(team, Team.Create(OtherTeamId, "Singapore Claims", "SG"));
        db.Users.AddRange(
            User.Create(ClaimantId, "Hafiz", UserRole.Claimant, "MY"),
            User.Create(OtherClaimantId, "Other claimant", UserRole.Claimant, "MY"),
            User.Create(OfficerId, "Ben", UserRole.ClaimsOfficer, "MY", TeamId),
            User.Create(OtherOfficerId, "Chen", UserRole.ClaimsOfficer, "MY", TeamId),
            User.Create(OtherMarketOfficerId, "Sam", UserRole.ClaimsOfficer, "SG", OtherTeamId),
            User.Create(ManagerId, "Aisha", UserRole.Manager, "MY", TeamId));
        await db.SaveChangesAsync();
    }

    internal static Claim Claim(
        Guid? claimantId = null,
        string market = "MY",
        string currency = "MYR",
        decimal loss = 1_000m,
        DateTimeOffset? submittedAt = null)
    {
        var time = submittedAt ?? Now.AddDays(-2);
        return ClaimsPlatform.Api.Claims.Domain.Claim.Submit(
            claimantId ?? ClaimantId,
            ClaimType.Motor,
            $"POL-{Guid.NewGuid():N}",
            market,
            currency,
            DateOnly.FromDateTime(time.UtcDateTime),
            "Kuala Lumpur",
            "Test incident",
            loss,
            time);
    }
}
