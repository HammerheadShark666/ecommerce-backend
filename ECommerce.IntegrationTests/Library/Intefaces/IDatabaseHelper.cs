using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.Library.Intefaces;

internal interface IDatabaseHelper
{
    Task<User> SeedUserAsync(SqlServerFixture fixture, string email, string password, bool isTwoFactor = false, string? oneTimePasswordSecret = null);
    DbContextOptions<ECommerceDbContext> CreateOptions(SqlServerFixture fixture);
}
