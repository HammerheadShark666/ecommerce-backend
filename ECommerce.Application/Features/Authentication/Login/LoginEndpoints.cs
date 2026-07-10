using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ECommerce.Application.Extensions;
using ECommerce.Application.Abstractions.Configuration;

namespace ECommerce.Application.Features.Authentication.Login;

public static class LoginEndpoints
{ 
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("")
                             .WithTags("Login");

        group.MapPost("/login", async ([FromBody] LoginRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        { 
            LoginResponse result = await mediator.Send(new LoginCommand(request.Email, request.Password));

            if(result.RequiresTwoFactor)
            {
                return Results.Ok(new
                {
                    RequiresTwoFactor = true,
                    result.PendingToken,
                    result.PendingTokenId
                });
            } 
            else
            {
                if (result.RefreshToken is not null)
                { 
                    response.SetRefreshToken(result.RefreshToken, jwtSettings.RefreshTokenExpiryDays);
                }

                return Results.Ok(new
                {
                    RequiresTwoFactor = false,
                    result.Token
                });
            }
        });

 
        return endpoints;
    }
}
