using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Authentication.Login;

public record LoginRequest(string Email, string Password);

public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

public record LoginResponse(
    bool RequiresTwoFactor,
    string? PendingToken,
    string? JwtToken,
    string? RefreshToken,
    Guid? PendingTokenId
);
