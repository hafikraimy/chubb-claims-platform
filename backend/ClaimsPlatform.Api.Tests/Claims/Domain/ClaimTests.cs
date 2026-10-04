using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Tests.Claims.Domain;

public class ClaimTests
{
    private static readonly Guid ClaimantId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly DateTimeOffset SubmittedAt =
        new(2026, 10, 3, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Submit_WithValidDetails_CreatesSubmittedClaim()
    {
        var claim = Claim.Submit(
            claimantId: ClaimantId,
            type: ClaimType.Motor,
            policyNumber: "POL-10001",
            market: "my",
            currency: "myr",
            incidentDate: new DateOnly(2026, 10, 2),
            incidentLocation: "Kuala Lumpur",
            description: "Rear bumper damaged in a collision.",
            reportedLossAmount: 2_500m,
            submittedAt: SubmittedAt);

        Assert.NotEqual(Guid.Empty, claim.Id);
        Assert.Equal(ClaimantId, claim.ClaimantId);
        Assert.Equal(ClaimType.Motor, claim.Type);
        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Equal("POL-10001", claim.PolicyNumber);
        Assert.Equal("MY", claim.Market);
        Assert.Equal("MYR", claim.Currency);
        Assert.Equal(2_500m, claim.ReportedLossAmount);
        Assert.Null(claim.AssessedLossAmount);
        Assert.Null(claim.AssignedOfficerId);
        Assert.Equal(SubmittedAt, claim.SubmittedAt);
        Assert.Equal(SubmittedAt, claim.UpdatedAt);
    }

    [Fact]
    public void Submit_WithFutureIncidentDate_ThrowsArgumentException()
    {
        var futureIncidentDate = new DateOnly(2026, 10, 4);

        var exception = Assert.Throws<ArgumentException>(() =>
            Claim.Submit(
                claimantId: ClaimantId,
                type: ClaimType.Property,
                policyNumber: "POL-10002",
                market: "MY",
                currency: "MYR",
                incidentDate: futureIncidentDate,
                incidentLocation: "Penang",
                description: "Water damage in the kitchen.",
                reportedLossAmount: 5_000m,
                submittedAt: SubmittedAt));

        Assert.Equal("incidentDate", exception.ParamName);
    }

    [Fact]
    public void Submit_WithNonPositiveReportedLoss_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Claim.Submit(
                claimantId: ClaimantId,
                type: ClaimType.Property,
                policyNumber: "POL-10003",
                market: "MY",
                currency: "MYR",
                incidentDate: new DateOnly(2026, 10, 2),
                incidentLocation: "Johor Bahru",
                description: "Storm damage to the roof.",
                reportedLossAmount: 0m,
                submittedAt: SubmittedAt));

        Assert.Equal("reportedLossAmount", exception.ParamName);
    }
}