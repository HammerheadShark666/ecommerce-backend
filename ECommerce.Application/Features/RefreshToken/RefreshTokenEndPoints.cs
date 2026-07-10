using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ECommerce.Application.Extensions;
using ECommerce.Core.Abstractions.Configuration;

namespace ECommerce.Application.Features.RefreshToken;

public static class RefreshTokenEndPoints
{
    public static IEndpointRouteBuilder MapRefreshTokenEndPoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/refresh-token")
                                                .WithTags("RefreshToken");

        group.MapPost("/", async (HttpRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) => {

            string? refreshToken =
                    request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            } 

            RefreshTokenResponse result = await mediator.Send(new RefreshTokenCommand(refreshToken));
            response.SetRefreshToken(result.RefreshToken, jwtSettings.RefreshTokenExpiryDays); 

            return Results.Ok(new RefreshTokenOnlyResponse(result.Token));
        });

        return endpoints;
    }
}
