namespace ECommerce.Application.Features.ForgottenPassword;
 
public record PasswordResetValidateRequest(string Token, string Email, string NewPassword, string Code);
