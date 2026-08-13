using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ECommerce.Application.Features.TwoFactorEnrolment.BeginEnableTwoFactorEnrolment;
using ECommerce.Application.Features.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;
using ECommerce.Application.Extensions;

namespace ECommerce.Application.Features.TwoFactorEnrolment;

public static class TwoFactorEnrolmentEndpoints
{  
    public static IEndpointRouteBuilder MapTwoFactorEnrolmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/2fa/enrol")
                             .WithTags("Two-Factor Enrolment");

        group.MapPost("", async (string email, IMediator mediator) =>
        {
            var result = await mediator.Send(new BeginTwoFactorEnrolmentCommand(email));
            return result.ToHttpResult();
        });

        group.MapPost("/confirm", async ([FromBody] ConfirmTwoFactorEnrolmentRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new ConfirmTwoFactorEnrolmentCommand(request.email, request.Code));
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
