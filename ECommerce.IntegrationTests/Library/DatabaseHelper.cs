using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library.Intefaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.Library;

public class DatabaseHelper : IDatabaseHelper
{
    public DbContextOptions<ECommerceDbContext> CreateOptions(SqlServerFixture fixture)
        => new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
 
    public async Task<User> SeedUserAsync(SqlServerFixture fixture, string email, string password, bool isTwoFactor = false, string? oneTimePasswordSecret = null)
    {
        DbContextOptions<ECommerceDbContext> options = CreateOptions(fixture);
        await using var db = new ECommerceDbContext(options);

        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(), 
            Email = email,
            PasswordHash = hash,
            IsTwoFactorEnabled = isTwoFactor,
            OneTimePasswordSecret = oneTimePasswordSecret,
            Status = "Active",
            LastName = "Smith",
            FirstName = "Alice",
            Phone = "1234567890",
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }
}
