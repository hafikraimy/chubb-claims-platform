using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Claims.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsPlatform.Api.Claims.Infrastructure;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claims");

        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Id)
            .HasColumnName("id");

        builder.Property(claim => claim.ReferenceNumber)
            .HasColumnName("reference_number")
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(claim => claim.ReferenceNumber)
            .IsUnique()
            .HasDatabaseName("ux_claims_reference_number");

        builder.Property(claim => claim.Version)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.Property(claim => claim.ClaimantId)
            .HasColumnName("claimant_id")
            .IsRequired();

        builder.Property(claim => claim.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(claim => claim.PolicyNumber)
            .HasColumnName("policy_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(claim => claim.Market)
            .HasColumnName("market")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(claim => claim.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(claim => claim.IncidentDate)
            .HasColumnName("incident_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(claim => claim.IncidentLocation)
            .HasColumnName("incident_location")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(claim => claim.Description)
            .HasColumnName("description")
            .HasMaxLength(2_000)
            .IsRequired();

        builder.Property(claim => claim.ReportedLossAmount)
            .HasColumnName("reported_loss_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(claim => claim.AssessedLossAmount)
            .HasColumnName("assessed_loss_amount")
            .HasPrecision(18, 2);

        builder.Property(claim => claim.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(claim => claim.AssignedOfficerId)
            .HasColumnName("assigned_officer_id");

        builder.Property(claim => claim.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(claim => claim.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    
        builder.Property(claim => claim.DecisionReason)
            .HasColumnName("decision_reason")
            .HasMaxLength(1_000);

        builder.Property(claim => claim.SettlementAmount)
            .HasColumnName("settlement_amount")
            .HasPrecision(18, 2);

        builder.HasIndex(claim => claim.ClaimantId)
            .HasDatabaseName("ix_claims_claimant_id");

        builder.HasIndex(claim => claim.AssignedOfficerId)
            .HasDatabaseName("ix_claims_assigned_officer_id");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(claim => claim.ClaimantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(claim => claim.AssignedOfficerId)
            .OnDelete(DeleteBehavior.Restrict);
    
        builder.HasMany(claim => claim.History)
            .WithOne()
            .HasForeignKey(history => history.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(claim => claim.History)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    
        builder.HasMany(claim => claim.InformationRequests)
            .WithOne()
            .HasForeignKey(request => request.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(claim => claim.InformationRequests)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
