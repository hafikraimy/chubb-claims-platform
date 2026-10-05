using ClaimsPlatform.Api.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClaimsPlatform.Api.Tests.Common;

public class ApiExceptionHandlerTests
{
    [Theory]
    [MemberData(nameof(KnownErrors))]
    public async Task TryHandleAsync_KnownException_WritesExpectedProblem(
        Exception exception,
        int expectedStatus,
        string expectedTitle,
        string expectedDetail)
    {
        var writer = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(
            writer,
            NullLogger<ApiExceptionHandler>.Instance);
        var context = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(context, exception, default);

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(expectedStatus, writer.Problem!.Status);
        Assert.Equal(expectedTitle, writer.Problem.Title);
        Assert.Equal(expectedDetail, writer.Problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_HidesInternalMessage()
    {
        var writer = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(
            writer,
            NullLogger<ApiExceptionHandler>.Instance);
        var context = new DefaultHttpContext();

        await handler.TryHandleAsync(
            context,
            new Exception("Sensitive internal detail"),
            default);

        Assert.Equal(StatusCodes.Status500InternalServerError, writer.Problem!.Status);
        Assert.Equal("An unexpected error occurred.", writer.Problem.Detail);
        Assert.DoesNotContain("Sensitive", writer.Problem.Detail);
    }

    public static TheoryData<Exception, int, string, string> KnownErrors => new()
    {
        {
            new ArgumentException("Amount is invalid."),
            StatusCodes.Status400BadRequest,
            "Validation failed",
            "Amount is invalid."
        },
        {
            new InvalidOperationException("Transition is not allowed."),
            StatusCodes.Status409Conflict,
            "Operation not allowed",
            "Transition is not allowed."
        },
        {
            new DbUpdateConcurrencyException(),
            StatusCodes.Status409Conflict,
            "Concurrency conflict",
            "The resource was updated by another request. Reload and try again."
        }
    };

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetails? Problem { get; private set; }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Problem = context.ProblemDetails;
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Problem = context.ProblemDetails;
            return ValueTask.CompletedTask;
        }
    }
}
