using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsPlatform.Api.Claims.Infrastructure;

public class ClaimHistoryConfiguration
    : IEntityTypeConfiguration<ClaimHistory>
{
    public void Configure(EntityTypeBuilder<ClaimHistory> builder)
    {
        builder.ToTable("claim_history");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id)
            .HasColumnName("id");

        builder.Property(history => history.ClaimId)
            .HasColumnName("claim_id")
            .IsRequired();

        builder.Property(history => history.ActingUserId)
            .HasColumnName("acting_user_id")
            .IsRequired();

        builder.Property(history => history.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(history => history.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(history => history.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(history => history.ActingUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => new
            {
                history.ClaimId,
                history.OccurredAt
            })
            .HasDatabaseName("ix_claim_history_claim_id_occurred_at");
    }
}