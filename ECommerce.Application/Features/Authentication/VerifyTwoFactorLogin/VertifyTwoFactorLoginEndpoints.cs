using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Authentication.VerifyTwoFactorLogin;

public static class VerifyTwoFactorLoginEndpoints
{
    public static IEndpointRouteBuilder MapVerifyTwoFactorLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/login")
                             .WithTags("VerifyTwoFactorLogin");

        group.MapPost("/2fa/verify", async ([FromBody] VerifyTwoFactorLoginRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) =>
        {
            VerifyTwoFactorLoginResponse result = await mediator.Send(new VerifyTwoFactorLoginCommand(request.Email, request.PendingToken, request.Code, request.PendingTokenId));

            string refreshToken = result.RefreshToken
                                        ?? throw new RefreshTokenMissingException();
           
            response.SetRefreshToken(refreshToken, jwtSettings.RefreshTokenExpiryDays); 
            
            return Results.Ok(new LoginResponse(result.Token));
        }).RequireRateLimiting(RateLimiterPolicyConstants.Login);

        return endpoints;
    }
}
