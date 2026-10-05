namespace ClaimsPlatform.Api.Claims.Domain;

public class Claim
{
    private readonly List<ClaimHistory> _history = [];
    private readonly List<InformationRequest> _informationRequests = [];
    private Claim()
    {
    }

    private Claim(
        Guid id,
        string referenceNumber,
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
        ReferenceNumber = referenceNumber;
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

    public string ReferenceNumber { get; private set; } = string.Empty;

    public uint Version { get; private set; }

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

    public IReadOnlyCollection<InformationRequest> InformationRequests => _informationRequests;

    public string? DecisionReason { get; private set; }

    public decimal? SettlementAmount { get; private set; }

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

        ValidateMaximumLength(policyNumber, ClaimFieldLimits.PolicyNumber, nameof(policyNumber));
        ValidateMaximumLength(market, ClaimFieldLimits.Market, nameof(market));
        ValidateMaximumLength(currency, ClaimFieldLimits.Currency, nameof(currency));
        ValidateMaximumLength(
            incidentLocation,
            ClaimFieldLimits.IncidentLocation,
            nameof(incidentLocation));
        ValidateMaximumLength(description, ClaimFieldLimits.Description, nameof(description));

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

        var id = Guid.NewGuid();
        var referenceNumber = $"CLM-{id:N}".ToUpperInvariant();

        return new Claim(
            id: id,
            referenceNumber: referenceNumber,
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

    public Guid RequestInformation(
        Guid officerId,
        string question,
        DateTimeOffset requestedAt)
    {
        if (AssignedOfficerId != officerId)
        {
            throw new InvalidOperationException(
                "Only the assigned officer can request information.");
        }

        if (Status != ClaimStatus.InReview)
        {
            throw new InvalidOperationException(
                "Information can only be requested for a claim in review.");
        }

        if (_informationRequests.Any(request => request.IsOpen))
        {
            throw new InvalidOperationException(
                "The claim already has an open information request.");
        }

        var informationRequest = InformationRequest.Create(
            claimId: Id,
            requestedByOfficerId: officerId,
            question: question,
            requestedAt: requestedAt);

        _informationRequests.Add(informationRequest);

        Status = ClaimStatus.AwaitingInfo;
        UpdatedAt = requestedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: officerId,
            eventType: ClaimHistoryEventType.InformationRequested,
            description: "Additional information requested.",
            occurredAt: requestedAt));

        return informationRequest.Id;
    }

    public void RespondToInformationRequest(
        Guid claimantId,
        Guid requestId,
        string response,
        DateTimeOffset respondedAt)
    {
        if (ClaimantId != claimantId)
        {
            throw new InvalidOperationException(
                "Only the owning claimant can provide information.");
        }

        if (Status != ClaimStatus.AwaitingInfo)
        {
            throw new InvalidOperationException(
                "This claim is not awaiting information.");
        }

        var informationRequest = _informationRequests
            .SingleOrDefault(request =>
                request.Id == requestId &&
                request.IsOpen);

        if (informationRequest is null)
        {
            throw new InvalidOperationException(
                "The open information request was not found.");
        }

        informationRequest.Respond(
            response: response,
            respondedAt: respondedAt);

        Status = ClaimStatus.InReview;
        UpdatedAt = respondedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: claimantId,
            eventType: ClaimHistoryEventType.InformationProvided,
            description: "Additional information provided.",
            occurredAt: respondedAt));
    }

    public void Settle(
        Guid officerId,
        decimal settlementAmount,
        string reason,
        DateTimeOffset decidedAt)
    {
        ValidateDecision(officerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ValidateMaximumLength(reason, ClaimFieldLimits.DecisionReason, nameof(reason));

        if (settlementAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settlementAmount),
                "Settlement amount must be greater than zero.");
        }

        Status = ClaimStatus.Settled;
        SettlementAmount = settlementAmount;
        DecisionReason = reason.Trim();
        UpdatedAt = decidedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: officerId,
            eventType: ClaimHistoryEventType.Settled,
            description: "Claim settled.",
            occurredAt: decidedAt));
    }

    public void Reject(
        Guid officerId,
        string reason,
        DateTimeOffset decidedAt)
    {
        ValidateDecision(officerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ValidateMaximumLength(reason, ClaimFieldLimits.DecisionReason, nameof(reason));

        Status = ClaimStatus.Rejected;
        SettlementAmount = null;
        DecisionReason = reason.Trim();
        UpdatedAt = decidedAt;

        _history.Add(ClaimHistory.Record(
            claimId: Id,
            actingUserId: officerId,
            eventType: ClaimHistoryEventType.Rejected,
            description: "Claim rejected.",
            occurredAt: decidedAt));
    }

    private void ValidateDecision(Guid officerId)
    {
        if (AssignedOfficerId != officerId)
        {
            throw new InvalidOperationException(
                "Only the assigned officer can decide this claim.");
        }

        if (Status != ClaimStatus.InReview)
        {
            throw new InvalidOperationException(
                "Only a claim in review can be settled or rejected.");
        }
    }

    private static void ValidateMaximumLength(string value, int maximumLength, string parameterName)
    {
        if (value.Trim().Length > maximumLength)
        {
            throw new ArgumentException(
                $"{parameterName} cannot exceed {maximumLength} characters.",
                parameterName);
        }
    }
}
