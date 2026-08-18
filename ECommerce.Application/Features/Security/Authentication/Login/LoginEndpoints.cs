using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Security.Authentication.Login;

public static class LoginEndpoints
{ 
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("")
                             .WithTags("Login");

        group.MapPost("/login", async ([FromBody] LoginRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        {
            var result = await mediator.Send(new LoginCommand(request.Email, request.Password));
            if (result.IsFailed)
            {
                return result.ToHttpResult();
            } 

            if (result.Value.RequiresTwoFactor)
            {
                return Results.Ok(new
                {
                    RequiresTwoFactor = true,
                    result.Value.PendingToken,
                    result.Value.PendingTokenId
                });
            }

            if (result.Value.RefreshToken is not null)
            {
                response.SetRefreshToken(
                    result.Value.RefreshToken,
                    jwtSettings.RefreshTokenExpiryDays);
            } 

            return Results.Ok(new
            {
                RequiresTwoFactor = false,
                result.Value.JwtToken
            }); 

        }).RequireRateLimiting(RateLimiterPolicyConstants.Login);
 
        return endpoints;
    }
}
