using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Authentication.Login;

public static class LoginEndpoints
{ 
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("")
                             .WithTags("Login");

        group.MapPost("/login", async ([FromBody] LoginRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        {
            var result = await mediator.Send(new LoginCommand(request.Email, request.Password));

            if (result.HasError<InvalidCredentialsError>())
            {
                var error = result.Errors
                    .OfType<InvalidCredentialsError>()
                    .First();

                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication failed",
                    detail: error.Message);
            }

            if (result.IsFailed)
            {
                return Results.BadRequest(new
                {
                    errors = result.Errors.Select(x => x.Message)
                });
            } 

            var login = result.Value;

            if (login.RequiresTwoFactor)
            {
                return Results.Ok(new
                {
                    RequiresTwoFactor = true,
                    login.PendingToken,
                    login.PendingTokenId
                });
            }

            if (login.RefreshToken is not null)
            {
                response.SetRefreshToken(
                    login.RefreshToken,
                    jwtSettings.RefreshTokenExpiryDays);
            }

            return Results.Ok(new
            {
                RequiresTwoFactor = false,
                login.Token
            }); 

        }).RequireRateLimiting(RateLimiterPolicyConstants.Login);
 
        return endpoints;
    }
}
