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
        RouteGroupBuilder group = endpoints.MapGroup("forgotten-password")
                             .WithTags("forgotten-password");

        group.MapPost("", async ([FromBody] ForgottenPasswordRequest request, IMediator mediator) => 
                                            await mediator.Send(new ForgottenPasswordCommand(request.Email)))
                     .RequireRateLimiting(RateLimiterPolicyConstants.ForgottonPassword);
        

         

        //group.MapGet("/reset/validate", async (string email, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        //{
        //    ResetPasswordValidateResponse result = await mediator.Send(new ResetPasswordValidateCommand(email));
        //});

        //group.MapPost("/reset", async ([FromBody] ResetPasswordRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        //{
        //    ResetPasswordResponse result = await mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword, request.TotpCode));
        //});

        return endpoints;
    }
}
