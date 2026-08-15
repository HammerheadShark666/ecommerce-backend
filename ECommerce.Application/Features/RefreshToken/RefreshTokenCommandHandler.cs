using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Domain.Entities.User;
using FluentResults; 
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.RefreshToken;

internal sealed class RefreshTokenCommandHandler(IECommerceDbContext dbContext, 
                                               IHmacsha256Hasher hmacsha256Hasher,
                                               IJwtGenerator jwtGenerator,
                                               TimeProvider timeProvider,
                                               IJwtSettings jwtSettings,
                                               IHashSettings hashSettings,
                                               IRefreshTokenGenerator refreshTokenGenerator) : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashedRefreshToken = hmacsha256Hasher.HashToken(request.RefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

        var refreshToken = await GetRefreshTokenRecord(hashedRefreshToken, cancellationToken);
        if(refreshToken is null || refreshToken.User is null)
        {
            return Result.Fail<RefreshTokenResponse>(new RefreshTokenNotFoundError());
        }
          
        (var accessToken, var newRefreshToken) = await GetNewTokensAsync(refreshToken.User, cancellationToken);

        await UpdateRefreshTokenTable(refreshToken, newRefreshToken, cancellationToken);

        return Result.Ok(new RefreshTokenResponse(newRefreshToken));
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
        var hashedRefreshToken = hmacsha256Hasher.HashToken(newRefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

        return new Domain.Entities.Authentication.RefreshToken
        {
            UserId = userId,
            Token = hashedRefreshToken,
            ExpiresAt = timeProvider.GetUtcNow().Add(refreshTokenLifeSpan).UtcDateTime
        };
    }

    private async Task<(string, string)> GetNewTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken =
           await jwtGenerator.GenerateTokenAsync(user, cancellationToken);

        var newRefreshToken =
            refreshTokenGenerator.GenerateRefreshToken();

        return (accessToken, newRefreshToken);
    }

    private async Task<Domain.Entities.Authentication.RefreshToken?> GetRefreshTokenRecord(string hashedRefreshToken,
                                                                         CancellationToken cancellationToken) => 
                    await dbContext.RefreshTokens
                        .Include(x => x.User)
                        .SingleOrDefaultAsync(
                            x => x.Token == hashedRefreshToken &&
                                    x.RevokedAt == null &&
                                    x.ExpiresAt > timeProvider.GetUtcNow().UtcDateTime,
                                cancellationToken);
}
