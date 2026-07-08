using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ECommerce.Application.Features.Authentication.Login;

namespace ECommerce.Application.Features.Authentication.Protected;

public static class ProtectedEndpoints
{
    public static IEndpointRouteBuilder MapProtectedEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/protected")
                             .WithTags("Protected")
                             .RequireAuthorization();

        group.MapGet("/me", (HttpContext http, [FromServices] ILogger<LoginCommandHandler> logger) =>
        { 
            ClaimsPrincipal user = http.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                logger.LogInformation("Protected Endpoint - Unauthenticated"); 
                return Results.Unauthorized();
            }

            var claims = user.Claims.Select(c => new { c.Type, c.Value });
            return Results.Ok(claims);
        });

        return endpoints;
    }
}
