using ECommerce.Core.Abstractions;
using ECommerce.Core.Abstractions.Configuration;
using ECommerce.Core.Constants;
using ECommerce.Domain.Entities.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;

public record RefreshTokenResponse(string Token, string RefreshToken);

public record RefreshTokenOnlyResponse(string? Token);

public sealed class RefreshTokenCommandHandler(IECommerceDbContext dbContext, 
                                               IHmacsha256Hasher hmacsha256Hasher,
                                               IJwtGenerator jwtGenerator,
                                               TimeProvider timeProvider,
                                               IJwtSettings jwtSettings,
                                               IHashSettings hashSettings,
                                               IRefreshTokenGenerator refreshTokenGenerator) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        string hashedRefreshToken = hmacsha256Hasher.HashToken(request.RefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

        Domain.Entities.Authentication.RefreshToken refreshToken = await GetRefreshTokenRecord(hashedRefreshToken, cancellationToken);

        User user = refreshToken.User
            ?? throw new UnauthorizedAccessException();

        (string? accessToken, string? newRefreshToken) = await GetNewTokensAsync(user);

        await UpdateRefreshTokenTable(refreshToken, newRefreshToken, cancellationToken);

        return new RefreshTokenResponse(accessToken, newRefreshToken);
    }

    private async Task UpdateRefreshTokenTable(Domain.Entities.Authentication.RefreshToken refreshToken, string newRefreshToken, CancellationToken cancellationToken)
    {
        refreshToken.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
        var refreshTokenLifeSpan
              = TimeSpan.FromDays(jwtSettings.RefreshTokenExpiryDays);

        await dbContext.RefreshTokens.AddAsync(
                CreateRefreshToken(refreshToken.User.Id, newRefreshToken, refreshTokenLifeSpan), cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new UnauthorizedAccessException();
        }
    }

    private Domain.Entities.Authentication.RefreshToken CreateRefreshToken(Guid userId, string newRefreshToken, TimeSpan refreshTokenLifeSpan)  
    {
        string hashedRefreshToken = hmacsha256Hasher.HashToken(newRefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

        return new Domain.Entities.Authentication.RefreshToken
        {
            UserId = userId,
            Token = hashedRefreshToken,
            ExpiresAt = timeProvider.GetUtcNow().Add(refreshTokenLifeSpan).UtcDateTime
        };
    }

    private async Task<(string, string)> GetNewTokensAsync(User user)
    {
        string accessToken =
           await jwtGenerator.GenerateTokenAsync(user);

        string newRefreshToken =
            refreshTokenGenerator.GenerateRefreshToken();

        return (accessToken, newRefreshToken);
    }

    private async Task<Domain.Entities.Authentication.RefreshToken> GetRefreshTokenRecord(string hashedRefreshToken, 
                                                                         CancellationToken cancellationToken)
    {
        Domain.Entities.Authentication.RefreshToken? refreshToken = await dbContext.RefreshTokens
                                                .Include(x => x.User)
                                                .SingleOrDefaultAsync(
                                                    x => x.Token == hashedRefreshToken &&
                                                         x.RevokedAt == null &&
                                                         x.ExpiresAt > timeProvider.GetUtcNow().UtcDateTime,
                                                        cancellationToken) ?? throw new UnauthorizedAccessException();         
        return refreshToken;
    }     
}
