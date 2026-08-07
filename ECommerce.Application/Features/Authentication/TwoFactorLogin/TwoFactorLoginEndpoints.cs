using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Authentication.TwoFactorLogin;

public static class TwoFactorLoginEndpoints
{
    public static IEndpointRouteBuilder MapTwoFactorLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/login")
                             .WithTags("VerifyTwoFactorLogin");

        group.MapPost("/2fa/verify", async ([FromBody] TwoFactorLoginCommand request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        {
            var result = await mediator.Send(new TwoFactorLoginCommand(request.Email, request.PendingToken, request.Code, request.PendingTokenId));

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



            //if (result.IsFailed)
            //{
            //    return Results.BadRequest(new
            //    {
            //        errors = result.Errors.Select(x => x.Message)
            //    });
            //}


            //?? throw new RefreshTokenMissingException();


            var refreshToken = result.Value.RefreshToken;
            response.SetRefreshToken(refreshToken, jwtSettings.RefreshTokenExpiryDays); 
            
            return Results.Ok(new LoginResponse(result.Value.Token));
        }).RequireRateLimiting(RateLimiterPolicyConstants.Login);

        return endpoints;
    }
}
