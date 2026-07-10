using System.Security.Cryptography;
using ECommerce.Core.Abstractions;
using ECommerce.Core.Abstractions.Configuration;
using ECommerce.Core.Abstractions.Messaging;
using ECommerce.Core.Constants;
using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.Application.Features.Authentication.Login;

public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

public record LoginResponse(
    bool RequiresTwoFactor,
    string? PendingToken,
    string? Token,
    string? RefreshToken,
    Guid? PendingTokenId
);

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

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        LoginResponse loginResponse;
        string normaliseEmail = request.Email.Trim().ToUpperInvariant(); 

        User user = await GetUserAsync(normaliseEmail, cancellationToken);

        ValidatePassword(user, request.Password); 

        if (user.IsTwoFactorEnabled)
        {
            await CloseExistingPendingTokens(user.Id, cancellationToken);
            (Guid pendingTokenId, string token) = await GenerateTwoFactorPendingTokenAsync(user.Id);
            loginResponse = new LoginResponse(true, token, null, null, pendingTokenId);
        }
        else
        {
            string refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken);
            string jwtToken = await jwtGenerator.GenerateTokenAsync(user);
            loginResponse = new LoginResponse(false, null, jwtToken, refreshToken, null);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return loginResponse;
    }

    private void ValidatePassword(User? user, string password)
    {
        string hash = user?.PasswordHash ?? AuthenticationConstants.DummyPasswordHash;

        bool validPassword = passwordHasher.Verify(password, hash);
        if (!validPassword || user is null)
        {
            throw new UnauthorizedAccessException();
        }

        return;
    }

    private async Task<(Guid, string)> GenerateTwoFactorPendingTokenAsync(Guid userId)
    { 
        string pendingToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));  
        string hashedPendingToken = hmacsha256Hasher.HashToken(pendingToken, AuthenticationConstants.HashTypeTokenPending, hashSettings.Secret);

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
        string refreshToken = refreshTokenGenerator.GenerateRefreshToken();
        string hashedRefreshToken = GetHashedRefreshToken(refreshToken);

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

    private async Task<User> GetUserAsync(string email, CancellationToken cancellationToken) =>
          await dbContext.Users
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
          ?? throw new UnauthorizedAccessException();
}
