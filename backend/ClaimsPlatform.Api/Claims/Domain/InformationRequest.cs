namespace ClaimsPlatform.Api.Claims.Domain;

public class InformationRequest
{
    private InformationRequest()
    {
    }

    private InformationRequest(
        Guid id,
        Guid claimId,
        Guid requestedByOfficerId,
        string question,
        DateTimeOffset requestedAt)
    {
        Id = id;
        ClaimId = claimId;
        RequestedByOfficerId = requestedByOfficerId;
        Question = question;
        RequestedAt = requestedAt;
    }

    public Guid Id { get; private set; }

    public Guid ClaimId { get; private set; }

    public Guid RequestedByOfficerId { get; private set; }

    public string Question { get; private set; } = string.Empty;

    public DateTimeOffset RequestedAt { get; private set; }

    public string? Response { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    public bool IsOpen => Response is null;

    internal static InformationRequest Create(
        Guid claimId,
        Guid requestedByOfficerId,
        string question,
        DateTimeOffset requestedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        return new InformationRequest(
            id: Guid.NewGuid(),
            claimId: claimId,
            requestedByOfficerId: requestedByOfficerId,
            question: question.Trim(),
            requestedAt: requestedAt);
    }

    internal void Respond(
        string response,
        DateTimeOffset respondedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response);

        if (!IsOpen)
        {
            throw new InvalidOperationException(
                "This information request has already been answered.");
        }

        Response = response.Trim();
        RespondedAt = respondedAt;
    }
}