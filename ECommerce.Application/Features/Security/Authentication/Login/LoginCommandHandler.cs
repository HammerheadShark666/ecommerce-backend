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

namespace ECommerce.Application.Features.Security.Authentication.Login;
 
internal class LoginCommandHandler(IECommerceDbContext dbContext,
                                   IPasswordHasher passwordHasher,
                                   IHmacsha256Hasher hmacsha256Hasher,
                                   TimeProvider timeProvider,
                                   IRefreshTokenGenerator refreshTokenGenerator,
                                   IJwtSettings jwtSettings,
                                   IHashSettings hashSettings,
                                   IJwtGenerator jwtGenerator) : ICommandHandler<LoginCommand, LoginResponse>
{
    private static readonly TimeSpan PendingTokenTtl = TimeSpan.FromMinutes(5);

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToUpperInvariant();

        var user = await GetUserAsync(email, cancellationToken);
        var authenticationResult = AuthenticateUser(user, request.Password);

        if (authenticationResult.IsFailed)
        {
            return Result.Fail<LoginResponse>(authenticationResult.Errors);
        }

        user = authenticationResult.Value;

        if (user.IsTwoFactorEnabled)
        {
            await CloseExistingPendingTokens(user.Id, cancellationToken);

            var (pendingTokenId, token) =
                await GenerateTwoFactorPendingTokenAsync(user.Id);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(
                new LoginResponse(
                    RequiresTwoFactor: true,
                    PendingToken: token,
                    JwtToken: null,
                    RefreshToken: null,
                    PendingTokenId: pendingTokenId));
        }

        var refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken);
        var jwtToken = await jwtGenerator.GenerateTokenAsync(user, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(
            new LoginResponse(
                RequiresTwoFactor: false,
                PendingToken: null,
                JwtToken: jwtToken,
                RefreshToken: refreshToken,
                PendingTokenId: null));
    }

    private Result<User> AuthenticateUser(User? user, string password)
    {
        var hash = user?.PasswordHash
            ?? AuthenticationConstants.DummyPasswordHash;

        var validPassword = passwordHasher.Verify(password, hash);

        if (!validPassword || user is null)
        {
            return Result.Fail<User>(
                new InvalidCredentialsError());
        }

        return Result.Ok(user);
    }
     
    private async Task<(Guid, string)> GenerateTwoFactorPendingTokenAsync(Guid userId)
    {
        var pendingToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hashedPendingToken = hmacsha256Hasher.HashToken(pendingToken, AuthenticationConstants.HashTypeTokenPending, hashSettings.Secret);

        var id = Guid.NewGuid();

        dbContext.PendingTwoFactorLogins.Add(new PendingTwoFactorLogin
        {
            Id = id,
            UserId = userId,
            PendingTwoFactorToken = hashedPendingToken,
            PendingTokenExpiresAt = timeProvider.GetUtcNow().Add(PendingTokenTtl),
            IsUsed = false
        });

        return (id, pendingToken);
    }

    private async Task CloseExistingPendingTokens(Guid userId, CancellationToken cancellationToken) => await dbContext.PendingTwoFactorLogins
            .Where(x => x.UserId == userId && !x.IsUsed)
            .ExecuteUpdateAsync(x => x.SetProperty(
                p => p.IsUsed,
                true),
                cancellationToken);

    private async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = GetHashedRefreshToken(refreshToken);

        await dbContext.RefreshTokens.AddAsync(
            new Domain.Entities.Authentication.RefreshToken
            {
                UserId = user.Id,
                Token = hashedRefreshToken,
                ExpiresAt = timeProvider.GetUtcNow().AddDays(jwtSettings.RefreshTokenExpiryDays).UtcDateTime
            }, cancellationToken);

        return refreshToken;
    }

    private string GetHashedRefreshToken(string refreshToken) =>
        hmacsha256Hasher.HashToken(refreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

    private Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                        dbContext.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken); 
}
          
