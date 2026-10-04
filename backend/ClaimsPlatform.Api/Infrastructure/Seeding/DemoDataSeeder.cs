using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Infrastructure.Seeding;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(
        ClaimsDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var team = Team.Create(
            id: DemoDataIds.MalaysiaTeam,
            name: "Malaysia Claims Team",
            market: "MY");

        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync(cancellationToken);

        var claimant = User.Create(
            id: DemoDataIds.Claimant,
            name: "Hafiz Claimant",
            role: UserRole.Claimant,
            market: "MY");

        var manager = User.Create(
            id: DemoDataIds.Manager,
            name: "Aisha Manager",
            role: UserRole.Manager,
            market: "MY",
            teamId: team.Id);

        var officerOne = User.Create(
            id: DemoDataIds.OfficerOne,
            name: "Ben Claims Officer",
            role: UserRole.ClaimsOfficer,
            market: "MY",
            teamId: team.Id);

        var officerTwo = User.Create(
            id: DemoDataIds.OfficerTwo,
            name: "Chen Claims Officer",
            role: UserRole.ClaimsOfficer,
            market: "MY",
            teamId: team.Id);

        dbContext.Users.AddRange(
            claimant,
            manager,
            officerOne,
            officerTwo);

        await dbContext.SaveChangesAsync(cancellationToken);

        team.AssignManager(manager.Id);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
