namespace ECommerce.Application.Features.Authentication.VerifyTwoFactorLogin;

public record VerifyTwoFactorLoginRequest(string Email, string PendingToken, string Code, Guid PendingTokenId);
