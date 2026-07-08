using ECommerce.Core.Abstractions;
using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options), IECommerceDbContext
{
	public DbSet<User> Users => Set<User>();
    public DbSet<PendingTwoFactorLogin> PendingTwoFactorLogins => Set<PendingTwoFactorLogin>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
	protected override void OnModelCreating(ModelBuilder modelBuilder){
		
		base.OnModelCreating(modelBuilder);
		
		modelBuilder.ApplyConfigurationsFromAssembly(
			typeof(ECommerceDbContext).Assembly);
	}
}
