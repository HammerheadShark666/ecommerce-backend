namespace ECommerce.Application.Features.ForgottenPassword;
 
public record ResetPasswordRequest(
    string? Token,
    string? NewPassword,    
    string? TotpCode
); 
