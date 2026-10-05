using ClaimsPlatform.Api.Claims.Domain;

namespace ClaimsPlatform.Api.Tests.Claims.Domain;

public class ClaimWorkflowTests
{
    private static readonly Guid ClaimantId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OfficerId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid OtherOfficerId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly Guid ManagerId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly DateTimeOffset SubmittedAt =
        new(2026, 10, 3, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void AssignTo_SameOfficer_RejectsDuplicateAssignmentWithoutChangingClaim()
    {
        var claim = CreateInReviewClaim();
        var updatedAt = claim.UpdatedAt;
        var historyCount = claim.History.Count;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.AssignTo(OfficerId, ManagerId, SubmittedAt.AddHours(2)));

        Assert.Equal("The claim is already assigned to this officer.", exception.Message);
        Assert.Equal(OfficerId, claim.AssignedOfficerId);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Equal(updatedAt, claim.UpdatedAt);
        Assert.Equal(historyCount, claim.History.Count);
    }

    [Theory]
    [InlineData(ClaimStatus.Settled)]
    [InlineData(ClaimStatus.Rejected)]
    public void AssignTo_CompletedClaim_RejectsReassignment(ClaimStatus completedStatus)
    {
        var claim = CreateInReviewClaim();
        Complete(claim, completedStatus);
        var updatedAt = claim.UpdatedAt;
        var historyCount = claim.History.Count;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.AssignTo(OtherOfficerId, ManagerId, SubmittedAt.AddHours(3)));

        Assert.Equal("A completed claim cannot be assigned.", exception.Message);
        Assert.Equal(OfficerId, claim.AssignedOfficerId);
        Assert.Equal(completedStatus, claim.Status);
        Assert.Equal(updatedAt, claim.UpdatedAt);
        Assert.Equal(historyCount, claim.History.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecordAssessedLoss_WithNonPositiveAmount_RejectsUpdate(decimal amount)
    {
        var claim = CreateInReviewClaim();
        var historyCount = claim.History.Count;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            claim.RecordAssessedLoss(OfficerId, amount, SubmittedAt.AddHours(2)));

        Assert.Equal("amount", exception.ParamName);
        Assert.Null(claim.AssessedLossAmount);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Equal(historyCount, claim.History.Count);
    }

    [Fact]
    public void RecordAssessedLoss_WhileAwaitingInformation_RejectsUpdate()
    {
        var claim = CreateAwaitingInformationClaim();
        var historyCount = claim.History.Count;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RecordAssessedLoss(OfficerId, 2_000m, SubmittedAt.AddHours(3)));

        Assert.Equal("Only a claim in review can be assessed.", exception.Message);
        Assert.Null(claim.AssessedLossAmount);
        Assert.Equal(ClaimStatus.AwaitingInfo, claim.Status);
        Assert.Equal(historyCount, claim.History.Count);
    }

    [Fact]
    public void RecordAssessedLoss_Twice_ReplacesEstimateAndAppendsHistory()
    {
        var claim = CreateInReviewClaim();

        claim.RecordAssessedLoss(OfficerId, 2_000m, SubmittedAt.AddHours(2));
        claim.RecordAssessedLoss(OfficerId, 1_750m, SubmittedAt.AddHours(3));

        Assert.Equal(1_750m, claim.AssessedLossAmount);
        Assert.Equal(SubmittedAt.AddHours(3), claim.UpdatedAt);
        Assert.Equal(
            2,
            claim.History.Count(entry =>
                entry.EventType == ClaimHistoryEventType.AssessedLossUpdated));
    }

    [Fact]
    public void RequestInformation_ByDifferentOfficer_RejectsRequest()
    {
        var claim = CreateInReviewClaim();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RequestInformation(
                OtherOfficerId,
                "Please provide the report.",
                SubmittedAt.AddHours(2)));

        Assert.Equal(
            "Only the assigned officer can request information.",
            exception.Message);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Empty(claim.InformationRequests);
    }

    [Fact]
    public void RequestInformation_WithBlankQuestion_RejectsRequest()
    {
        var claim = CreateInReviewClaim();
        var historyCount = claim.History.Count;

        var exception = Assert.Throws<ArgumentException>(() =>
            claim.RequestInformation(OfficerId, "   ", SubmittedAt.AddHours(2)));

        Assert.Equal("question", exception.ParamName);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Empty(claim.InformationRequests);
        Assert.Equal(historyCount, claim.History.Count);
    }

    [Fact]
    public void RequestInformation_WhileAlreadyAwaiting_RejectsSecondRequest()
    {
        var claim = CreateAwaitingInformationClaim();
        var historyCount = claim.History.Count;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RequestInformation(
                OfficerId,
                "A second question.",
                SubmittedAt.AddHours(3)));

        Assert.Equal(
            "Information can only be requested for a claim in review.",
            exception.Message);
        Assert.Equal(ClaimStatus.AwaitingInfo, claim.Status);
        Assert.Single(claim.InformationRequests);
        Assert.Equal(historyCount, claim.History.Count);
    }

    [Fact]
    public void RespondToInformationRequest_WithUnknownRequest_RejectsResponse()
    {
        var claim = CreateAwaitingInformationClaim();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RespondToInformationRequest(
                ClaimantId,
                Guid.NewGuid(),
                "Police report PR-123.",
                SubmittedAt.AddHours(3)));

        Assert.Equal("The open information request was not found.", exception.Message);
        Assert.Equal(ClaimStatus.AwaitingInfo, claim.Status);
        Assert.True(claim.InformationRequests.Single().IsOpen);
    }

    [Fact]
    public void RespondToInformationRequest_WithBlankResponse_RejectsResponse()
    {
        var claim = CreateAwaitingInformationClaim();
        var request = claim.InformationRequests.Single();

        var exception = Assert.Throws<ArgumentException>(() =>
            claim.RespondToInformationRequest(
                ClaimantId,
                request.Id,
                "   ",
                SubmittedAt.AddHours(3)));

        Assert.Equal("response", exception.ParamName);
        Assert.Equal(ClaimStatus.AwaitingInfo, claim.Status);
        Assert.True(request.IsOpen);
    }

    [Fact]
    public void RespondToInformationRequest_WhenNotAwaiting_RejectsResponse()
    {
        var claim = CreateInReviewClaim();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RespondToInformationRequest(
                ClaimantId,
                Guid.NewGuid(),
                "Unexpected response.",
                SubmittedAt.AddHours(2)));

        Assert.Equal("This claim is not awaiting information.", exception.Message);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
    }

    [Fact]
    public void RespondToInformationRequest_Twice_RejectsSecondResponse()
    {
        var claim = CreateAwaitingInformationClaim();
        var requestId = claim.InformationRequests.Single().Id;
        claim.RespondToInformationRequest(
            ClaimantId,
            requestId,
            "Police report PR-123.",
            SubmittedAt.AddHours(3));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.RespondToInformationRequest(
                ClaimantId,
                requestId,
                "Changed response.",
                SubmittedAt.AddHours(4)));

        Assert.Equal("This claim is not awaiting information.", exception.Message);
        Assert.Equal("Police report PR-123.", claim.InformationRequests.Single().Response);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
    }

    [Theory]
    [InlineData(ClaimStatus.Settled)]
    [InlineData(ClaimStatus.Rejected)]
    public void Decide_WithBlankReason_RejectsDecision(ClaimStatus decision)
    {
        var claim = CreateInReviewClaim();

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            if (decision == ClaimStatus.Settled)
            {
                claim.Settle(OfficerId, 2_000m, "   ", SubmittedAt.AddHours(2));
            }
            else
            {
                claim.Reject(OfficerId, "   ", SubmittedAt.AddHours(2));
            }
        });

        Assert.Equal("reason", exception.ParamName);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Null(claim.DecisionReason);
        Assert.Null(claim.SettlementAmount);
    }

    [Fact]
    public void Reject_ByDifferentOfficer_RejectsDecision()
    {
        var claim = CreateInReviewClaim();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            claim.Reject(
                OtherOfficerId,
                "Attempted rejection.",
                SubmittedAt.AddHours(2)));

        Assert.Equal(
            "Only the assigned officer can decide this claim.",
            exception.Message);
        Assert.Equal(ClaimStatus.InReview, claim.Status);
        Assert.Null(claim.DecisionReason);
    }

    [Fact]
    public void FullSettlementWorkflow_RecordsOrderedAuditHistory()
    {
        var claim = CreateSubmittedClaim();
        claim.AssignTo(OfficerId, ManagerId, SubmittedAt.AddHours(1));
        claim.RecordAssessedLoss(OfficerId, 2_000m, SubmittedAt.AddHours(2));
        var requestId = claim.RequestInformation(
            OfficerId,
            "Please provide the police report reference.",
            SubmittedAt.AddHours(3));
        claim.RespondToInformationRequest(
            ClaimantId,
            requestId,
            "Police report PR-123.",
            SubmittedAt.AddHours(4));
        claim.Settle(
            OfficerId,
            1_900m,
            "Covered repair approved.",
            SubmittedAt.AddHours(5));

        Assert.Equal(ClaimStatus.Settled, claim.Status);
        Assert.Equal(2_000m, claim.AssessedLossAmount);
        Assert.Equal(1_900m, claim.SettlementAmount);
        Assert.Equal(
            [
                ClaimHistoryEventType.Submitted,
                ClaimHistoryEventType.Assigned,
                ClaimHistoryEventType.AssessedLossUpdated,
                ClaimHistoryEventType.InformationRequested,
                ClaimHistoryEventType.InformationProvided,
                ClaimHistoryEventType.Settled,
            ],
            claim.History.Select(entry => entry.EventType));
        Assert.Equal(
            [
                SubmittedAt,
                SubmittedAt.AddHours(1),
                SubmittedAt.AddHours(2),
                SubmittedAt.AddHours(3),
                SubmittedAt.AddHours(4),
                SubmittedAt.AddHours(5),
            ],
            claim.History.Select(entry => entry.OccurredAt));
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
        claim.AssignTo(OfficerId, OfficerId, SubmittedAt.AddHours(1));
        return claim;
    }

    private static Claim CreateAwaitingInformationClaim()
    {
        var claim = CreateInReviewClaim();
        claim.RequestInformation(
            OfficerId,
            "Please provide the police report reference.",
            SubmittedAt.AddHours(2));
        return claim;
    }

    private static void Complete(Claim claim, ClaimStatus status)
    {
        if (status == ClaimStatus.Settled)
        {
            claim.Settle(
                OfficerId,
                2_000m,
                "Repair approved.",
                SubmittedAt.AddHours(2));
            return;
        }

        claim.Reject(
            OfficerId,
            "Claim rejected.",
            SubmittedAt.AddHours(2));
    }
}
