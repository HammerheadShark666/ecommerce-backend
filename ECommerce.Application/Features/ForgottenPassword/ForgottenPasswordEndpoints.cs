using ECommerce.Application.Common;
using ECommerce.Application.Constants;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.ForgottenPassword;

public static class ForgottenPasswordEndpoints
{
    public static IEndpointRouteBuilder MapForgottenPasswordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("forgotten-password")
                             .WithTags("forgotten-password");

        group.MapPost("", async ([FromBody] ForgottenPasswordRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new ForgottenPasswordCommand(request.Email));
            return result.ToHttpResult();

        }).RequireRateLimiting(RateLimiterPolicyConstants.ForgottonPassword);
        
        group.MapPost("/reset/validate", async ([FromBody] PasswordResetValidateRequest request, IMediator mediator, HttpContext httpContext) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var result = await mediator.Send(new PasswordResetValidateCommand(request.Token, request.Email, request.NewPassword, request.Code, ipAddress));

            return result.ToHttpResult();
        })                                            
        .RequireRateLimiting(RateLimiterPolicyConstants.ForgottonPassword); 

        return endpoints;
    }
}
