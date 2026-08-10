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
        var client = appFactory.CreateClient();

        var email = "forgotten@example.com";

        // insert a user with normalized email (handler normalizes to upper-case)
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
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


        var request = new HttpRequestMessage(
                                HttpMethod.Post,
                                "/forgotten-password")
        {
            Content = JsonContent.Create(new { Email = email })
        };

        request.Headers.TryAddWithoutValidation(
            "X-Forwarded-For",
            "192.168.1.100");

        var resp = await client.SendAsync(request);



        // Act
       // var resp = await client.PostAsJsonAsync("/forgotten-password", new { Email = email });

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
        var client = appFactory.CreateClient();

        var email = "unknownuser@example.com";

        // Ensure no user exists with this email
        // Act
       // var resp = await client.PostAsJsonAsync("/forgotten-password", new { Email = email });


        var request = new HttpRequestMessage(
                               HttpMethod.Post,
                               "/forgotten-password")
        {
            Content = JsonContent.Create(new { Email = email })
        };

        request.Headers.TryAddWithoutValidation(
            "X-Forwarded-For",
            "192.168.1.100");

        var resp = await client.SendAsync(request);




        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        var problem = (ValidationProblemDetails?)(await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>()
            ?? await resp.Content.ReadFromJsonAsync<ProblemDetails>());

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(401);
        problem.Title.Should().Be("Unauthorized");
    }
}
