using Serilog;
using Serilog.Context;

namespace ECommerce.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId =
            context.Request.Headers[HeaderName].FirstOrDefault()
            ?? context.TraceIdentifier
            ?? Guid.NewGuid().ToString();

        context.TraceIdentifier = correlationId;

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
