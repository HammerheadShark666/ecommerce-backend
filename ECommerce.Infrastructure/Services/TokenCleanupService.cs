using ECommerce.Application.Abstractions;
using ECommerce.Infrastructure.Services.Intefaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public sealed class TokenCleanupService(IECommerceDbContext dbContext,
                                        TimeProvider timeProvider) : ITokenCleanupService
{ 
    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        await dbContext.RefreshTokens
            .Where(x =>
                x.RevokedAt != null ||
                x.ExpiresAt <= now)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.PendingTwoFactorLogins
            .Where(x =>
                x.IsUsed ||
                x.PendingTokenExpiresAt <= now)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
