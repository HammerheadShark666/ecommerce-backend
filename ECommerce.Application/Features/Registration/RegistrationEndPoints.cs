using ECommerce.Application.Constants;
using ECommerce.Application.Features.Registration.BeginRegistration;
using ECommerce.Application.Features.Registration.RequestRegistrationVerifyEmail;
using ECommerce.Application.Features.Registration.VerifyRegistration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
        }).RequireRateLimiting(RateLimiterPolicyConstants.Register);

        group.MapPost("/verify-email", async ([FromBody] VerifyRegistrationRequest request, IMediator mediator) =>
        {
            VerifyRegistrationResponse result = await mediator.Send(new VerifyRegistrationCommand(request.Email, request.Code));
            return Results.Ok(result);
        }).RequireRateLimiting(RateLimiterPolicyConstants.Register);

        group.MapPost("/request-verify-email", async ([FromBody] RequestVerifyRegistrationEmailRequest request, IMediator mediator) =>
        {
            RequestVerifyRegistrationEmailResponse result = await mediator.Send(new RequestVerifyRegistrationEmailCommand(request.Email));
            return Results.Ok(result);
        }).RequireRateLimiting(RateLimiterPolicyConstants.Register);


        return endpoints;
    }
}
