using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;

public record RefreshTokenResponse(string RefreshToken); 
