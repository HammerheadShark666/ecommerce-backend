using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.Registration.VerifyRegistration;

public record VerifyRegistrationRequest(string Email, string Code);

public record VerifyRegistrationCommand(string Email, string Code) : ICommand<VerifyRegistrationResponse>;

public record VerifyRegistrationResponse(string Message = "Registration email verified");
