using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.ForgottenPassword;

public record ForgottenPasswordRequest(string Email);

public record ForgottenPasswordCommand(string Email) : ICommand<ForgottenPasswordResponse>;

public record ForgottenPasswordResponse(string Message);
