using ECommerce.Application.Constants;
using ECommerce.Application.Extensions;
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
        var group = endpoints.MapGroup("/register")
                             .WithTags("Registration");

        group.MapPost("", async ([FromBody] BeginRegistrationRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new BeginRegistrationCommand(request.Email, request.Password, request.ConfirmPassword,
                                                                          request.LastName, request.FirstName, request.PhoneNumber));  
            return result.ToHttpResult();
        }).RequireRateLimiting(RateLimiterPolicyConstants.Register);

        group.MapPost("/request-verify-email", async ([FromBody] RequestVerifyRegistrationEmailRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new RequestVerifyRegistrationEmailCommand(request.Email));
            return result.ToHttpResult();
        }).RequireRateLimiting(RateLimiterPolicyConstants.Register);

        group.MapPost("/verify-email", async ([FromBody] VerifyRegistrationRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new VerifyRegistrationCommand(request.Email, request.Code));
            return result.ToHttpResult();
        }).RequireRateLimiting(RateLimiterPolicyConstants.Register);        

        return endpoints;
    }
}
