using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Infrastructure.Persistence;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) 
    : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<InformationRequest> InformationRequests =>
    Set<InformationRequest>();
    public DbSet<ClaimHistory> ClaimHistoryEntries => Set<ClaimHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ClaimsDbContext).Assembly);
    }
}
