using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Middleware.ExceptionHandlers;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,        
        CancellationToken cancellationToken)
    {
        string correlationId = httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}",
            correlationId); 

        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
