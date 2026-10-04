using ClaimsPlatform.Api.Access.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsPlatform.Api.Access.Infrastructure;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(team => team.Id);

        builder.Property(team => team.Id)
            .HasColumnName("id");

        builder.Property(team => team.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(team => team.Market)
            .HasColumnName("market")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(team => team.ManagerId)
            .HasColumnName("manager_id");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Team>(team => team.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}