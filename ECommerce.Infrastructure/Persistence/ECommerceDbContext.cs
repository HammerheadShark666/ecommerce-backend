using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.PasswordReset;
using ECommerce.Domain.Entities.Product;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options), IECommerceDbContext
{
	public DbSet<User> Users => Set<User>();
    public DbSet<PendingTwoFactorLogin> PendingTwoFactorLogins => Set<PendingTwoFactorLogin>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Product> Products => Set<Domain.Entities.Product.Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder){
		
		base.OnModelCreating(modelBuilder);
		
		modelBuilder.ApplyConfigurationsFromAssembly(
			typeof(ECommerceDbContext).Assembly);
	}
}
