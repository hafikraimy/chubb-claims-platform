using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsPlatform.Api.Claims.Infrastructure;

public class InformationRequestConfiguration
    : IEntityTypeConfiguration<InformationRequest>
{
    public void Configure(EntityTypeBuilder<InformationRequest> builder)
    {
        builder.ToTable("information_requests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Id)
            .HasColumnName("id");

        builder.Property(request => request.ClaimId)
            .HasColumnName("claim_id")
            .IsRequired();

        builder.Property(request => request.RequestedByOfficerId)
            .HasColumnName("requested_by_officer_id")
            .IsRequired();

        builder.Property(request => request.Question)
            .HasColumnName("question")
            .HasMaxLength(1_000)
            .IsRequired();

        builder.Property(request => request.RequestedAt)
            .HasColumnName("requested_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(request => request.Response)
            .HasColumnName("response")
            .HasMaxLength(2_000);

        builder.Property(request => request.RespondedAt)
            .HasColumnName("responded_at")
            .HasColumnType("timestamp with time zone");

        builder.Ignore(request => request.IsOpen);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.RequestedByOfficerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}