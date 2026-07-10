using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.IntegrationTests;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<SqlServerFixture>
{
}

[Collection("Database")] 
public class DatabaseTest(SqlServerFixture fixture)
{
    private readonly SqlServerFixture _fixture = fixture;

    public async Task GetUser_Should_Return_Seeded_Data()
    {
        // Arrange
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var context = new ECommerceDbContext(options);

        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == "alice");
            
            
         user.Should().NotBeNull();
         user.Email.Should().Be("alice");
         
        user = await context.Users.FirstOrDefaultAsync(u => u.Email == "john");

        user.Should().NotBeNull(); 

        user.Email = "john.updated@example.com";
        user.Email = "johnny"; // if you want to change username

        await context.SaveChangesAsync();
    }
}
