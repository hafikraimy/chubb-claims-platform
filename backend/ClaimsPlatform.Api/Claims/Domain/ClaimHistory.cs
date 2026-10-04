namespace ClaimsPlatform.Api.Claims.Domain;

public class ClaimHistory
{
    private ClaimHistory()
    {
    }

    private ClaimHistory(
        Guid id,
        Guid claimId,
        Guid actingUserId,
        ClaimHistoryEventType eventType,
        string description,
        DateTimeOffset occurredAt)
    {
        Id = id;
        ClaimId = claimId;
        ActingUserId = actingUserId;
        EventType = eventType;
        Description = description;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public Guid ClaimId { get; private set; }

    public Guid ActingUserId { get; private set; }

    public ClaimHistoryEventType EventType { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public static ClaimHistory Record(
        Guid claimId,
        Guid actingUserId,
        ClaimHistoryEventType eventType,
        string description,
        DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new ClaimHistory(
            id: Guid.NewGuid(),
            claimId: claimId,
            actingUserId: actingUserId,
            eventType: eventType,
            description: description.Trim(),
            occurredAt: occurredAt);
    }
}