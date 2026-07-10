using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.CheckTwoFactorStatus;

public static class TwoFactorStatusEndpoints
{  
    public static IEndpointRouteBuilder MapTwoFactorStatusEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/2fa")
                             .WithTags("Two-Factor Authentication");

        group.MapGet("/status", async (string email, IMediator mediator) =>
        {
            GetTwoFactorStatusResponse result = await mediator.Send(new GetTwoFactorStatusQuery(email));
            return Results.Ok(result);
        });

        return endpoints;
    }
}
