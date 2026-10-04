namespace ClaimsPlatform.Api.Claims.Domain;

public enum ClaimHistoryEventType
{
    Submitted,
    Assigned,
    Reassigned,
    AssessedLossUpdated,
    InformationRequested,
    InformationProvided,
    Settled,
    Rejected
}
