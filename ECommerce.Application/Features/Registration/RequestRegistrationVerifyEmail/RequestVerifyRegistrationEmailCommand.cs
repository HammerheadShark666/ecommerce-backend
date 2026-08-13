using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Registration.RequestRegistrationVerifyEmail;

public record RequestVerifyRegistrationEmailRequest(string Email);

public record RequestVerifyRegistrationEmailCommand(string Email) : ICommand<RequestVerifyRegistrationEmailResponse>;

public record RequestVerifyRegistrationEmailResponse(
    string Message = "Send verify registration email initiated successfully."
);
