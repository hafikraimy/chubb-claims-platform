using ClaimsPlatform.Api.Access.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsPlatform.Api.Access.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id");

        builder.Property(user => user.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(user => user.Market)
            .HasColumnName("market")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(user => user.TeamId)
            .HasColumnName("team_id");

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(user => user.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(user => user.TeamId)
            .HasDatabaseName("ix_users_team_id");
    }
}