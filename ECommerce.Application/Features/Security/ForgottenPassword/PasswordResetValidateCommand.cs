using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.ForgottenPassword;
 
public record PasswordResetValidateCommand(string Token, string Email, string NewPassword, string Code, string IpAddress) : ICommand<PasswordResetValidateResponse>;

public record PasswordResetValidateResponse(string Message);

public record PasswordResetValidateRequest(string Token, string Email, string NewPassword, string Code);
