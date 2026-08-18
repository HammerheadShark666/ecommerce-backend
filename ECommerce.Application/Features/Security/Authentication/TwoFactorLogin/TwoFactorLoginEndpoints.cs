using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Security.Authentication.TwoFactorLogin;

public static class TwoFactorLoginEndpoints
{
    public static IEndpointRouteBuilder MapTwoFactorLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/login")
                             .WithTags("VerifyTwoFactorLogin");

        group.MapPost("/2fa/verify", async ([FromBody] TwoFactorLoginCommand request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        {
            var result = await mediator.Send(new TwoFactorLoginCommand(request.Email, request.PendingToken, request.Code, request.PendingTokenId));
            if (result.IsFailed)
            {
                return result.ToHttpResult();
            } 

            var refreshToken = result.Value.RefreshToken;
            response.SetRefreshToken(refreshToken, jwtSettings.RefreshTokenExpiryDays); 

            return Results.Ok(new
            {
                RequiresTwoFactor = false,
                result.Value.Token
            });

        }).RequireRateLimiting(RateLimiterPolicyConstants.Login);

        return endpoints;
    }
}
