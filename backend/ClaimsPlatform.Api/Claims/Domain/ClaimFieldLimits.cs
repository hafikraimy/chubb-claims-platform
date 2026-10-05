namespace ClaimsPlatform.Api.Claims.Domain;

public static class ClaimFieldLimits
{
    public const int PolicyNumber = 50;
    public const int Market = 2;
    public const int Currency = 3;
    public const int IncidentLocation = 200;
    public const int Description = 2_000;
    public const int InformationQuestion = 1_000;
    public const int InformationResponse = 2_000;
    public const int DecisionReason = 1_000;
}
