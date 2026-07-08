using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ECommerce.Application.Features.TwoFactorEnrolment.BeginEnableTwoFactorEnrolment;
using ECommerce.Application.Features.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;

namespace ECommerce.Application.Features.TwoFactorEnrolment;

public static class TwoFactorEnrolmentEndpoints
{  
    public static IEndpointRouteBuilder MapTwoFactorEnrolmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/2fa/enrol")
                             .WithTags("Two-Factor Enrolment");

        group.MapPost("", async (string email, IMediator mediator) =>
        {
            BeginTwoFactorEnrolmentResponse result = await mediator.Send(new BeginTwoFactorEnrolmentCommand(email));
            return Results.Ok(result);
        });

        group.MapPost("/confirm", async ([FromBody] ConfirmTwoFactorEnrolmentRequest request, IMediator mediator) =>
        {
            ConfirmTwoFactorEnrolmentResponse result = await mediator.Send(new ConfirmTwoFactorEnrolmentCommand(request.email, request.Code));
            return Results.Ok(result);
        });

        return endpoints;
    }
}
