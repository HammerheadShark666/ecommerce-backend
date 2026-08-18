using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Security.RefreshToken;

public static class RefreshTokenEndPoints
{ 
    public static IEndpointRouteBuilder MapRefreshTokenEndPoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/refresh-token")
                                                .WithTags("RefreshToken");

        group.MapPost("/", async (HttpRequest request, IMediator mediator, HttpResponse response, IJwtSettings jwtSettings) => {

            var refreshToken = request.Cookies["refreshToken"] ?? "";
             
            var result = await mediator.Send(new RefreshTokenCommand(refreshToken));
            if (result.IsSuccess)
            {
                response.SetRefreshToken(result.Value.RefreshToken, jwtSettings.RefreshTokenExpiryDays);
            } 

            return result.ToHttpResult();
        });

        return endpoints;
    }
}
