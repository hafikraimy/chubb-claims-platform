using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Tests.Claims.Domain;

public class ClaimTests
{
    private static readonly Guid OfficerId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid OtherOfficerId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");
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
    
        var history = Assert.Single(claim.History);

        Assert.Equal(ClaimHistoryEventType.Submitted, history.EventType);
        Assert.Equal(ClaimantId, history.ActingUserId);
        Assert.Equal("Claim submitted.", history.Description);
        Assert.Equal(SubmittedAt, history.OccurredAt);
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

    [Fact]
    public void AssignTo_SubmittedClaim_AssignsOfficerAndStartsReview()
    {
        var claim = CreateSubmittedClaim();
        var assignedAt = SubmittedAt.AddHours(1);

        claim.AssignTo(
            officerId: OfficerId,
            actingUserId: OfficerId,
            assignedAt: assignedAt);

        Assert.Equal(OfficerId, claim.AssignedOfficerId);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Equal(assignedAt, claim.UpdatedAt);

        var history = claim.History.Last();

        Assert.Equal(ClaimHistoryEventType.Assigned, history.EventType);
        Assert.Equal(OfficerId, history.ActingUserId);
        Assert.Equal(assignedAt, history.OccurredAt);
    }

    [Fact]
    public void RecordAssessedLoss_ByDifferentOfficer_ThrowsInvalidOperationException()
    {
        var claim = CreateSubmittedClaim();

        claim.AssignTo(
            officerId: OfficerId,
            actingUserId: OfficerId,
            assignedAt: SubmittedAt.AddHours(1));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RecordAssessedLoss(
                officerId: OtherOfficerId,
                amount: 2_000m,
                assessedAt: SubmittedAt.AddHours(2)));

        Assert.Equal(
            "Only the assigned officer can assess this claim.",
            exception.Message);

        Assert.Null(claim.AssessedLossAmount);
    }

    [Fact]
    public void RecordAssessedLoss_ByAssignedOfficer_UpdatesAmount()
    {
        var claim = CreateSubmittedClaim();

        claim.AssignTo(
            officerId: OfficerId,
            actingUserId: OfficerId,
            assignedAt: SubmittedAt.AddHours(1));

        var assessedAt = SubmittedAt.AddHours(2);

        claim.RecordAssessedLoss(
            officerId: OfficerId,
            amount: 2_000m,
            assessedAt: assessedAt);

        Assert.Equal(2_000m, claim.AssessedLossAmount);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Equal(assessedAt, claim.UpdatedAt);

        var history = claim.History.Last();

        Assert.Equal(
            ClaimHistoryEventType.AssessedLossUpdated,
            history.EventType);

        Assert.Equal(OfficerId, history.ActingUserId);
        Assert.Equal(assessedAt, history.OccurredAt);
    }

    [Fact]
    public void RequestInformation_ByAssignedOfficer_MovesClaimToAwaitingInfo()
    {
        var claim = CreateInReviewClaim();
        var requestedAt = SubmittedAt.AddHours(2);

        var requestId = claim.RequestInformation(
            officerId: OfficerId,
            question: "Please provide the police report reference.",
            requestedAt: requestedAt);

        Assert.NotEqual(Guid.Empty, requestId);
        Assert.Equal(ClaimStatus.AwaitingInfo, claim.Status);
        Assert.Equal(requestedAt, claim.UpdatedAt);

        var request = Assert.Single(claim.InformationRequests);

        Assert.Equal(requestId, request.Id);
        Assert.True(request.IsOpen);
        Assert.Null(request.Response);
        Assert.Equal(
            "Please provide the police report reference.",
            request.Question);
    }

    [Fact]
    public void RespondToInformationRequest_ByOwningClaimant_ReturnsClaimToReview()
    {
        var claim = CreateInReviewClaim();

        var requestId = claim.RequestInformation(
            officerId: OfficerId,
            question: "Please provide the police report reference.",
            requestedAt: SubmittedAt.AddHours(2));

        var respondedAt = SubmittedAt.AddHours(3);

        claim.RespondToInformationRequest(
            claimantId: ClaimantId,
            requestId: requestId,
            response: "The police report reference is PR-12345.",
            respondedAt: respondedAt);

        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Equal(respondedAt, claim.UpdatedAt);

        var request = Assert.Single(claim.InformationRequests);

        Assert.False(request.IsOpen);
        Assert.Equal(
            "The police report reference is PR-12345.",
            request.Response);
        Assert.Equal(respondedAt, request.RespondedAt);

        var history = claim.History.Last();

        Assert.Equal(
            ClaimHistoryEventType.InformationProvided,
            history.EventType);
        Assert.Equal(ClaimantId, history.ActingUserId);
    }

    [Fact]
    public void RespondToInformationRequest_ByDifferentClaimant_ThrowsInvalidOperationException()
    {
        var claim = CreateInReviewClaim();

        var requestId = claim.RequestInformation(
            officerId: OfficerId,
            question: "Please provide the police report reference.",
            requestedAt: SubmittedAt.AddHours(2));

        var otherClaimantId =
            Guid.Parse("44444444-4444-4444-4444-444444444444");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RespondToInformationRequest(
                claimantId: otherClaimantId,
                requestId: requestId,
                response: "Attempted response.",
                respondedAt: SubmittedAt.AddHours(3)));

        Assert.Equal(
            "Only the owning claimant can provide information.",
            exception.Message);

        Assert.Equal(ClaimStatus.AwaitingInfo, claim.Status);
        Assert.True(claim.InformationRequests.Single().IsOpen);
    }

    private static Claim CreateSubmittedClaim()
    {
        return Claim.Submit(
            claimantId: ClaimantId,
            type: ClaimType.Motor,
            policyNumber: "POL-10001",
            market: "MY",
            currency: "MYR",
            incidentDate: new DateOnly(2026, 10, 2),
            incidentLocation: "Kuala Lumpur",
            description: "Rear bumper damaged in a collision.",
            reportedLossAmount: 2_500m,
            submittedAt: SubmittedAt);
    }

    private static Claim CreateInReviewClaim()
    {
        var claim = CreateSubmittedClaim();

        claim.AssignTo(
            officerId: OfficerId,
            actingUserId: OfficerId,
            assignedAt: SubmittedAt.AddHours(1));

        return claim;
    }
}