using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.PasswordReset;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Abstractions;

public interface IECommerceDbContext
{
    DbSet<User> Users { get; }
    DbSet<PendingTwoFactorLogin> PendingTwoFactorLogins { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
