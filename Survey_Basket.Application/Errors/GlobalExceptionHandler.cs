using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Survey_Basket.Application.Errors;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred. {Message} ", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Title = "Internal server error.",
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }
}
