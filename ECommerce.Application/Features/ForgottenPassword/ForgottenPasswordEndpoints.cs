using ECommerce.Application.Constants;
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
        RouteGroupBuilder group = endpoints.MapGroup("/forgotten-password")
                             .WithTags("forgotten-password");

        group.MapPost("", async ([FromBody] ForgottenPasswordRequest request, IMediator mediator) => 
                                            await mediator.Send(new ForgottenPasswordCommand(request.Email)))
                     .RequireRateLimiting(RateLimiterPolicyConstants.ForgottonPassword);

        group.MapPost("/reset/validate", async ([FromBody] PasswordResetValidateRequest request, IMediator mediator, HttpContext httpContext) =>
        {
            string? ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            await mediator.Send(new PasswordResetValidateCommand(request.Token, request.Email, request.NewPassword, request.Code, ipAddress));
        })                                            
        .RequireRateLimiting(RateLimiterPolicyConstants.ForgottonPassword); 

        return endpoints;
    }
}
