using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities.User;
using ECommerce.Domain.Entities.Authentication;

namespace ECommerce.Application.Abstractions;

public interface IECommerceDbContext
{
    DbSet<User> Users { get; }
    DbSet<PendingTwoFactorLogin> PendingTwoFactorLogins { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
