using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimsPlatform.Api.Common.Errors;

public class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Validation failed"),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Operation not allowed"),

            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Concurrency conflict"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception switch
                    {
                        DbUpdateConcurrencyException =>
                            "The resource was updated by another request. " +
                            "Reload and try again.",

                        _ when statusCode ==
                            StatusCodes.Status500InternalServerError =>
                            "An unexpected error occurred.",

                        _ => exception.Message
                    }
                }
            });
    }
}
