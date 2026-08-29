using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.Authentication.TwoFactorLogin;
 
public record TwoFactorLoginCommand(string Email, string PendingToken, string Code, Guid PendingTokenId) : ICommand<TwoFactorLoginResponse>;

public record TwoFactorLoginResponse(string? JwtToken, string RefreshToken);
