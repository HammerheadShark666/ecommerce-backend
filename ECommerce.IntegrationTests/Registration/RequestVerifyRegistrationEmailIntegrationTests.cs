using System.Net.Http.Json;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.IntegrationTests.Registration;

[Collection("Database")]
public class RequestVerifyRegistrationEmailIntegrationTests(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RequestVerifyEmail_WhenUserExistsAndNotVerified_PublishesMessage()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        var email = "requestverify@example.com";

        // insert a user who is not email verified
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using (var db = new ECommerceDbContext(options))
        {
            db.Users.Add(new User
            {
                Email = email,
                FirstName = "Req",
                LastName = "User",
                PasswordHash = "hash",
                Phone = "000",
                Status = "Pending",
                IsEmailVerified = false
            });

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.PostAsJsonAsync("/register/request-verify-email", new { Email = email });

        // Assert
        resp.EnsureSuccessStatusCode();

        // Verify a VerifyRegistrationEmail message was published
        appFactory.Publisher.PublishedMessages.Should().ContainSingle(m => m.GetType().Name == "VerifyRegistrationEmail");
    }

    [Fact]
    public async Task RequestVerifyEmail_WhenAlreadyVerified_ReturnsUnauthorized()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        var email = "alreadyverified@example.com";

        // insert a user who is already verified
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using (var db = new ECommerceDbContext(options))
        {
            db.Users.Add(new User
            {
                Email = email,
                FirstName = "Req",
                LastName = "User",
                PasswordHash = "hash",
                Phone = "000",
                Status = "Active",
                IsEmailVerified = true
            });

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.PostAsJsonAsync("/register/request-verify-email", new { Email = email });

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);

        var problem = (ValidationProblemDetails?)(await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>()
            ?? await resp.Content.ReadFromJsonAsync<ProblemDetails>());

        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("This registration has already been verified.");
    }
}
