using System.ComponentModel.DataAnnotations;
using ClaimsPlatform.Api.Claims.Domain;
using ClaimsPlatform.Api.Claims.Dtos;

namespace ClaimsPlatform.Api.Tests.Claims.Dtos;

public class RequestValidationTests
{
    [Theory]
    [InlineData(typeof(SubmitClaimRequest), "PolicyNumber", ClaimFieldLimits.PolicyNumber)]
    [InlineData(typeof(SubmitClaimRequest), "IncidentLocation", ClaimFieldLimits.IncidentLocation)]
    [InlineData(typeof(SubmitClaimRequest), "Description", ClaimFieldLimits.Description)]
    [InlineData(typeof(CreateInformationRequest), "Question", ClaimFieldLimits.InformationQuestion)]
    [InlineData(typeof(SettleClaimRequest), "Reason", ClaimFieldLimits.DecisionReason)]
    [InlineData(typeof(RejectClaimRequest), "Reason", ClaimFieldLimits.DecisionReason)]
    [InlineData(typeof(RespondToInformationRequestRequest), "Response", ClaimFieldLimits.InformationResponse)]
    public void RequestRecords_DefineLengthValidationOnPrimaryConstructor(
        Type requestType,
        string parameterName,
        int expectedMaximumLength)
    {
        var parameter = requestType
            .GetConstructors()
            .Single()
            .GetParameters()
            .Single(item => string.Equals(
                item.Name,
                parameterName,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(parameter.GetCustomAttributes(typeof(RequiredAttribute), false).SingleOrDefault());
        var maximumLength = Assert.Single(
            parameter.GetCustomAttributes(typeof(MaxLengthAttribute), false)
                .Cast<MaxLengthAttribute>());
        Assert.Equal(expectedMaximumLength, maximumLength.Length);
    }

    [Fact]
    public void SubmitClaim_CurrencyValidationIsOnPrimaryConstructor()
    {
        var parameter = typeof(SubmitClaimRequest)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Single(item => item.Name == "Currency");

        var length = Assert.Single(
            parameter.GetCustomAttributes(typeof(StringLengthAttribute), false)
                .Cast<StringLengthAttribute>());
        Assert.Equal(ClaimFieldLimits.Currency, length.MaximumLength);
        Assert.Equal(ClaimFieldLimits.Currency, length.MinimumLength);
    }
}
