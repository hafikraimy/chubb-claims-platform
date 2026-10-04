namespace ClaimsPlatform.Api.Claims.Domain;

public class Claim
{
    private readonly List<ClaimHistory> _history = [];
    private Claim()
    {
    }

    private Claim(
        Guid id,
        Guid claimantId,
        ClaimType type,
        string policyNumber,
        string market,
        string currency,
        DateOnly incidentDate,
        string incidentLocation,
        string description,
        decimal reportedLossAmount,
        DateTimeOffset submittedAt)
    {
        Id = id;
        ClaimantId = claimantId;
        Type = type;
        PolicyNumber = policyNumber;
        Market = market;
        Currency = currency;
        IncidentDate = incidentDate;
        IncidentLocation = incidentLocation;
        Description = description;
        ReportedLossAmount = reportedLossAmount;
        Status = ClaimStatus.Submitted;
        SubmittedAt = submittedAt;
        UpdatedAt = submittedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: ClaimantId,
            eventType: ClaimHistoryEventType.Submitted,
            description: "Claim submitted.",
            occurredAt: SubmittedAt)
        );
    }

    public Guid Id { get; private set; }

    public Guid ClaimantId { get; private set; }

    public ClaimType Type { get; private set; }

    public string PolicyNumber { get; private set; } = string.Empty;

    public string Market { get; private set; } = string.Empty;

    public string Currency { get; private set; } = string.Empty;

    public DateOnly IncidentDate { get; private set; }

    public string IncidentLocation { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal ReportedLossAmount { get; private set; }

    public decimal? AssessedLossAmount { get; private set; }

    public ClaimStatus Status { get; private set; }

    public Guid? AssignedOfficerId { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ClaimHistory> History => _history;

    public static Claim Submit(
        Guid claimantId,
        ClaimType type,
        string policyNumber,
        string market,
        string currency,
        DateOnly incidentDate,
        string incidentLocation,
        string description,
        decimal reportedLossAmount,
        DateTimeOffset submittedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(market);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(incidentLocation);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (claimantId == Guid.Empty)
        {
            throw new ArgumentException(
                "A claimant is required.",
                nameof(claimantId));
        }

        if (currency.Trim().Length != 3)
        {
            throw new ArgumentException(
                "Currency must be a three-character ISO code.",
                nameof(currency));
        }

        if (incidentDate > DateOnly.FromDateTime(submittedAt.UtcDateTime))
        {
            throw new ArgumentException(
                "Incident date cannot be in the future.",
                nameof(incidentDate));
        }

        if (reportedLossAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reportedLossAmount),
                "Reported loss amount must be greater than zero.");
        }

        return new Claim(
            id: Guid.NewGuid(),
            claimantId: claimantId,
            type: type,
            policyNumber: policyNumber.Trim(),
            market: market.Trim().ToUpperInvariant(),
            currency: currency.Trim().ToUpperInvariant(),
            incidentDate: incidentDate,
            incidentLocation: incidentLocation.Trim(),
            description: description.Trim(),
            reportedLossAmount: reportedLossAmount,
            submittedAt: submittedAt);
    }

    public void AssignTo(
        Guid officerId,
        Guid actingUserId,
        DateTimeOffset assignedAt)
    {
        if (officerId == Guid.Empty)
        {
            throw new ArgumentException(
                "An officer Id is required.",
                nameof(officerId)
            );
        }

        if (actingUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "An acting user ID is required.",
                nameof(actingUserId));
        }

        if (Status is ClaimStatus.Settled or ClaimStatus.Rejected)
        {
            throw new InvalidOperationException(
                "A completed claim cannot be assigned.");
        }

        if (AssignedOfficerId == officerId)
        {
            throw new InvalidOperationException(
                "The claim is already assigned to this officer.");
        }

        var eventType = AssignedOfficerId is null
        ? ClaimHistoryEventType.Assigned
        : ClaimHistoryEventType.Reassigned;

        AssignedOfficerId = officerId;

        if (Status == ClaimStatus.Submitted)
        {
            Status = ClaimStatus.InReview;
        }

        UpdatedAt = assignedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: actingUserId,
            eventType: eventType,
            description: eventType == ClaimHistoryEventType.Assigned
                ? "Claim assigned to an officer."
                : "Claim reassigned to another officer.",
            occurredAt: assignedAt));
    }

    public void RecordAssessedLoss(
        Guid officerId,
        decimal amount,
        DateTimeOffset assessedAt)
    {
        if (AssignedOfficerId != officerId)
        {
            throw new InvalidOperationException(
                "Only the assigned officer can assess this claim.");
        }

        if (Status != ClaimStatus.InReview)
        {
            throw new InvalidOperationException(
                "Only a claim in review can be assessed.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Assessed loss amount must be greater than zero.");
        }

        AssessedLossAmount = amount;
        UpdatedAt = assessedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: officerId,
            eventType: ClaimHistoryEventType.AssessedLossUpdated,
            description: "Assessed loss amount updated.",
            occurredAt: assessedAt));
    }
}