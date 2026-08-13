using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.ForgottenPassword;

public record ForgottenPasswordRequest(string Email);

public record ForgottenPasswordCommand(string Email) : ICommand<ForgottenPasswordResponse>;

public record ForgottenPasswordResponse(string Message);
