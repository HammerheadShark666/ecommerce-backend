using System.Security.Cryptography;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Authentication.TwoFactorLogin;
 
internal class TwoFactorLoginCommandHandler(IECommerceDbContext dbContext,
                                            TimeProvider timeProvider,
                                            IHmacsha256Hasher hmacsha256Hasher,
                                            IOneTimePasswordGenerator oneTimePasswordGenerator,
                                            IAesEncryptionHelper aesEncryptionHelper,                                     
                                            IRefreshTokenGenerator refreshTokenGenerator,
                                            IJwtSettings jwtSettings,
                                            IHashSettings hashSettings,                                                 
                                            IEncryptionSettings encryptionSettings,
                                            IJwtGenerator jwtGenerator,
                                            ILogger<TwoFactorLoginCommandHandler> logger) : ICommandHandler<TwoFactorLoginCommand, TwoFactorLoginResponse>
{ 
    public async Task<Result<TwoFactorLoginResponse>> Handle(TwoFactorLoginCommand request, CancellationToken cancellationToken)
    {
        var normaliseEmail = request.Email.Trim().ToUpperInvariant();

        var user = await GetUserAsync(normaliseEmail, cancellationToken);
        if (user is null)
        {
            return Result.Fail<TwoFactorLoginResponse>(new InvalidCredentialsError());
        } 

        var pendingTwoFactorLogin = await ValidateTwoFactorPendingToken(request.PendingToken, request.PendingTokenId, cancellationToken);
        if (pendingTwoFactorLogin.IsFailed)
        {
            return Result.Fail<TwoFactorLoginResponse>(pendingTwoFactorLogin.Errors);
        } 

        var validateTwoFactorCodeResult = await ValidateTwoFactorCodeAsync(user.OneTimePasswordSecret, request.Code, user.Id);
        if (validateTwoFactorCodeResult.IsFailed)
        {
            return Result.Fail<TwoFactorLoginResponse>(validateTwoFactorCodeResult.Errors);
        }

        await ClearPendingTokenAsync(pendingTwoFactorLogin.Value);       

        var refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken); 
        var jwtToken = await jwtGenerator.GenerateTokenAsync(user, cancellationToken); 

        return Result.Ok(new TwoFactorLoginResponse(jwtToken, refreshToken));
    }

    private async Task ClearPendingTokenAsync(PendingTwoFactorLogin pendingTwoFactorLogin)
    {
        pendingTwoFactorLogin.IsUsed = true;
        await dbContext.SaveChangesAsync();
        return;
    } 
    
    private Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                        dbContext.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    private async Task<Result<PendingTwoFactorLogin>> ValidateTwoFactorPendingToken(string pendingToken, Guid pendingTokenId, CancellationToken cancellationToken)
    {
        var pendingTwoFactorLogin = await dbContext.PendingTwoFactorLogins
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(x =>
                                                     x.Id == pendingTokenId,
                                                     cancellationToken);
    
        if (pendingTwoFactorLogin is null || pendingTwoFactorLogin.PendingTwoFactorToken is null)
        {
            return Result.Fail<PendingTwoFactorLogin>(
                new InvalidCredentialsError());
        }

        ValidatePendingToken(pendingToken, pendingTwoFactorLogin.PendingTwoFactorToken);         

        var cleanUpPendingTokenResult = await CleanUpPendingTokenAsync(pendingTwoFactorLogin);
        if (cleanUpPendingTokenResult.IsFailed)
        {
            return Result.Fail(cleanUpPendingTokenResult.Errors);
        }

        return Result.Ok(pendingTwoFactorLogin);
    }

    private async Task<Result> CleanUpPendingTokenAsync(PendingTwoFactorLogin pendingTwoFactorLogin)
    {
        if (!pendingTwoFactorLogin.PendingTokenExpiresAt.HasValue || pendingTwoFactorLogin.PendingTokenExpiresAt.Value < DateTime.UtcNow)
        {
            await ClearPendingTokenAsync(pendingTwoFactorLogin);             
            return Result.Fail(new InvalidCredentialsError());            
        }

        return Result.Ok();
    }

    private Result ValidatePendingToken(string pendingToken, string storedPendingToken)
    {
        var incomingHashedToken =
            hmacsha256Hasher.HashToken(
                pendingToken,
                AuthenticationConstants.HashTypeTokenPending,
                hashSettings.Secret);

        var incomingHashBytes =
            Convert.FromBase64String(incomingHashedToken);

        var storedHashBytes =
            Convert.FromBase64String(storedPendingToken);

        if (!CryptographicOperations.FixedTimeEquals(incomingHashBytes, storedHashBytes))
        { 
            return Result.Fail(new InvalidCredentialsError());
        }

        return Result.Ok();
    }

    private async Task<Result> ValidateTwoFactorCodeAsync(string? oneTimePasswordSecret, string code, Guid userId)
    { 
        if (oneTimePasswordSecret is null)
        {
            logger.LogError("One time password secret missing for user {UserId}", userId);
            return Result.Fail(new InvalidCredentialsError());
        }

        var oneTimePassEncryptionKey = encryptionSettings.OneTimePasswordKey;
        var decryptedOneTimePasswordSecret = aesEncryptionHelper.Decrypt(oneTimePasswordSecret, oneTimePassEncryptionKey);

        if (!oneTimePasswordGenerator.VerifyCode(decryptedOneTimePasswordSecret, code))
        {
            return Result.Fail(new InvalidCredentialsError());
        }

        return Result.Ok();
    }

    private async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = hmacsha256Hasher.HashToken(refreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

        await dbContext.RefreshTokens.AddAsync(
          new Domain.Entities.Authentication.RefreshToken
          {
              UserId = user.Id,
              Token = hashedRefreshToken,
              ExpiresAt = timeProvider.GetUtcNow()
                                  .AddDays(jwtSettings.RefreshTokenExpiryDays).UtcDateTime

          }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken); 

        return refreshToken;
    }
}
