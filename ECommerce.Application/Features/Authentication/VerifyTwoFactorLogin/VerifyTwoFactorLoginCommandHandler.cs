using System.Security.Cryptography;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Constants;
using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Authentication.VerifyTwoFactorLogin;

public record VerifyTwoFactorLoginCommand(string Email, string PendingToken, string Code, Guid PendingTokenId) : ICommand<VerifyTwoFactorLoginResponse>;

public record VerifyTwoFactorLoginResponse(string? Token, string RefreshToken);

public record LoginResponse(string? Token);

internal class VerifyTwoFactorLoginCommandHandler(IECommerceDbContext dbContext,
                                                  TimeProvider timeProvider,
                                                  IHmacsha256Hasher hmacsha256Hasher,
                                                  IOneTimePasswordGenerator oneTimePasswordGenerator,
                                                  IAesEncryptionHelper aesEncryptionHelper,                                     
                                                  IRefreshTokenGenerator refreshTokenGenerator,
                                                  IJwtSettings jwtSettings,
                                                  IHashSettings hashSettings,                                                 
                                                  IEncryptionSettings encryptionSettings,
                                                  IJwtGenerator jwtGenerator) : ICommandHandler<VerifyTwoFactorLoginCommand, VerifyTwoFactorLoginResponse>
{ 
    public async Task<VerifyTwoFactorLoginResponse> Handle(VerifyTwoFactorLoginCommand request, CancellationToken cancellationToken)
    {        
        string normaliseEmail = request.Email.Trim().ToUpperInvariant();

        User user = await GetUserAsync(normaliseEmail, cancellationToken);

        PendingTwoFactorLogin pendingTwoFactorLogin = await ValidateTwoFactorPendingToken(request.PendingToken, request.PendingTokenId, cancellationToken);

        await ValidateTwoFactorCodeAsync(user.OneTimePasswordSecret, request.Code);
        await ClearPendingTokenAsync(pendingTwoFactorLogin);

        string refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken);
        string jwtToken = await jwtGenerator.GenerateTokenAsync(user);

        return new VerifyTwoFactorLoginResponse(jwtToken, refreshToken);
    }

    private async Task ClearPendingTokenAsync(PendingTwoFactorLogin pendingTwoFactorLogin)
    {
        pendingTwoFactorLogin.IsUsed = true;
        await dbContext.SaveChangesAsync();
        return;
    }

    private async Task<User> GetUserAsync(string email, CancellationToken cancellationToken) => 
           await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
           ?? throw new UnauthorizedAccessException();

    private async Task<PendingTwoFactorLogin> ValidateTwoFactorPendingToken(string pendingToken, Guid pendingTokenId, CancellationToken cancellationToken)
    {
        PendingTwoFactorLogin? pendingTwoFactorLogin = await dbContext.PendingTwoFactorLogins
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(x =>
                                                     x.Id == pendingTokenId,                                                    
                                                     cancellationToken) ?? throw new UnauthorizedAccessException();
       
        if (pendingTwoFactorLogin.PendingTwoFactorToken is null)
        {            
            throw new UnauthorizedAccessException();
        }

        ValidatePendingToken(pendingToken, pendingTwoFactorLogin.PendingTwoFactorToken);
        await CleanUpPendingTokenAsync(pendingTwoFactorLogin);

        return pendingTwoFactorLogin;
    }

    private async Task CleanUpPendingTokenAsync(PendingTwoFactorLogin pendingTwoFactorLogin)
    {
        if (!pendingTwoFactorLogin.PendingTokenExpiresAt.HasValue || pendingTwoFactorLogin.PendingTokenExpiresAt.Value < DateTime.UtcNow)
        {
            await ClearPendingTokenAsync(pendingTwoFactorLogin);
            throw new UnauthorizedAccessException();
        }
    }

    private void ValidatePendingToken(string pendingToken, string storedPendingToken)
    {
        string incomingHashedToken =
            hmacsha256Hasher.HashToken(
                pendingToken,
                "pending",
                hashSettings.Secret);

        byte[] incomingHashBytes =
            Convert.FromBase64String(incomingHashedToken);

        byte[] storedHashBytes =
            Convert.FromBase64String(storedPendingToken);

        if (!CryptographicOperations.FixedTimeEquals(
                incomingHashBytes,
                storedHashBytes))
        {
            throw new UnauthorizedAccessException();
        }
    }

    private async Task ValidateTwoFactorCodeAsync(string? oneTimePasswordSecret, string code)
    {
        if (oneTimePasswordSecret == null)
        {
            throw new UnauthorizedAccessException();
        }
       
        string oneTimePassEncryptionKey = encryptionSettings.OneTimePasswordKey;
        string decryptedOneTimePasswordSecret = aesEncryptionHelper.Decrypt(oneTimePasswordSecret, oneTimePassEncryptionKey);

        if (!oneTimePasswordGenerator.VerifyCode(decryptedOneTimePasswordSecret, code))
        {           
            throw new UnauthorizedAccessException();
        }
    }

    private async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        string refreshToken = refreshTokenGenerator.GenerateRefreshToken();
        string hashedRefreshToken = hmacsha256Hasher.HashToken(refreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

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
