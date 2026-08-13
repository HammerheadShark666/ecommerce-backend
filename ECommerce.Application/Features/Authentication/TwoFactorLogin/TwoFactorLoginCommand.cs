using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Authentication.TwoFactorLogin;
 
public record TwoFactorLoginCommand(string Email, string PendingToken, string Code, Guid PendingTokenId) : ICommand<TwoFactorLoginResponse>;

public record TwoFactorLoginResponse(string? Token, string RefreshToken);

public record LoginResponse(string? Token);
