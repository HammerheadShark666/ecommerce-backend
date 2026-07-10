using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ECommerce.Application.Features.Registration.BeginRegistration;
using ECommerce.Application.Features.Registration.VerifyRegistration;

namespace ECommerce.Application.Features.Registration;

public static class RegistrationEndpoints
{
    public static IEndpointRouteBuilder MapRegistrationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/register")
                             .WithTags("Registration");

        group.MapPost("", async ([FromBody] BeginRegistrationRequest request, IMediator mediator) =>
        {
            BeginRegistrationResponse result = await mediator.Send(new BeginRegistrationCommand(request.Email, request.Password, request.ConfirmPassword,
                                                                                                    request.LastName, request.FirstName, request.PhoneNumber));
            return Results.Ok(result);
        });

        group.MapPost("/verify", async ([FromBody] VerifyRegistrationRequest request, IMediator mediator) =>
        {
            VerifyRegistrationResponse result = await mediator.Send(new VerifyRegistrationCommand(request.Email, request.Code));
            return Results.Ok(result);
        });

        return endpoints;
    }
}
