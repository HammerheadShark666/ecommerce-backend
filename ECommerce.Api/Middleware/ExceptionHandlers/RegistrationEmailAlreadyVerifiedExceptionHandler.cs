using ECommerce.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Middleware.ExceptionHandlers;
 
internal sealed class RegistrationEmailAlreadyVerifiedExceptionHandler
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RegistrationEmailAlreadyVerifiedException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Registration verification email",
            Detail = "The registration verification email has already been verified."
        }, cancellationToken);

        return true;
    }
}
