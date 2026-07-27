using System.Net.Http.Json;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.IntegrationTests.ForgottenPassword;

[Collection("Database")]
public class ForgottenPasswordIntegrationTests(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ForgottenPassword_WhenUserExists_PublishesPasswordResetRequested()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        HttpClient client = appFactory.CreateClient();

        string email = "forgotten@example.com";

        // insert a user with normalized email (handler normalizes to upper-case)
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using (var db = new ECommerceDbContext(options))
        {
            db.Users.Add(new User
            {
                Email = email.Trim().ToUpperInvariant(),
                FirstName = "Forgot",
                LastName = "User",
                PasswordHash = "hash",
                Phone = "000",
                Status = "Active",
                IsEmailVerified = true
            });

            await db.SaveChangesAsync();
        }

        // Act
        HttpResponseMessage resp = await client.PostAsJsonAsync("/forgotten-password", new { Email = email });

        // Assert
        resp.EnsureSuccessStatusCode();

        // Verify PasswordResetRequested message published
        appFactory.Publisher.PublishedMessages.Should().ContainSingle(m => m.GetType().Name == "PasswordResetRequested");
    }

    [Fact]
    public async Task ForgottenPassword_WhenUserDoesNotExist_ReturnsUnauthorized()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        HttpClient client = appFactory.CreateClient();

        string email = "unknownuser@example.com";

        // Ensure no user exists with this email
        // Act
        HttpResponseMessage resp = await client.PostAsJsonAsync("/forgotten-password", new { Email = email });

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        var problem = (ValidationProblemDetails?)(await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>()
            ?? await resp.Content.ReadFromJsonAsync<ProblemDetails>());

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(401);
        problem.Title.Should().Be("Unauthorised");
    }
}
