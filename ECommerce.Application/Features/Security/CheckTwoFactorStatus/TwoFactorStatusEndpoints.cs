using ECommerce.Application.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus;

public static class TwoFactorStatusEndpoints
{  
    public static IEndpointRouteBuilder MapTwoFactorStatusEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/2fa")
                             .WithTags("ECommerce");

        group.MapGet("/status", async (string email, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTwoFactorStatusQuery(email)); 
            return result.ToHttpResult();  
        });

        return endpoints;
    }
}
