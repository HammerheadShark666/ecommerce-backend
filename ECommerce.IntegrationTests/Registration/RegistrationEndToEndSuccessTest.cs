using System.Net.Http.Json;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.IntegrationTests.Registration;

[Collection("Database")]
public class RegistrationEndToEndSuccessTest(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EndToEnd_Register_PublishesMessage_FunctionIsSimulated_UpdateDb_And_Verify_Succeeds()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        HttpClient client = appFactory.CreateClient();

        string email = "sbtest@example.com";
        string password = "RegPass!1";

        // Act: begin registration
        HttpResponseMessage beginResp = await client.PostAsJsonAsync("/register", new { Email = email, Password = password, ConfirmPassword = password, LastName = "Smith", FirstName = "John", PhoneNumber = "01924 4323432" });
        beginResp.EnsureSuccessStatusCode();

        // Ensure a message was published to be processed by the background function
        appFactory.Publisher.PublishedMessages.Should().ContainSingle();

        // Simulate the Azure Function processing the published message by generating the
        // verification code, hashing it and updating the user record in the database.
        const string fixedCode = "123456"; // deterministic code for test

        using (IServiceScope scope = appFactory.Services.CreateScope())
        {
            IHmacsha256Hasher hmac = scope.ServiceProvider.GetRequiredService<IHmacsha256Hasher>();
            IHashSettings hashSettings = scope.ServiceProvider.GetRequiredService<IHashSettings>();

            string hashedCode = hmac.HashToken(fixedCode, RegistrationConstants.HashTypeVerifyRegistrationEmail, hashSettings.Secret);

            ECommerceDbContext db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

            // User emails are stored upper-case in the DB during creation
            string storedEmail = email.Trim().ToUpperInvariant();
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == storedEmail);
            user.Should().NotBeNull();

            user!.EmailVerificationCode = hashedCode;
            user.EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(RegistrationConstants.VerifyRegistrationEmailExpiryMinutes);
            await db.SaveChangesAsync();
        }

        // Act: call verification endpoint with the plain code
        HttpResponseMessage verifyResp = await client.PostAsJsonAsync("/register/verify-email", new { Email = email, Code = fixedCode });
        verifyResp.EnsureSuccessStatusCode();

        // Assert: user updated in DB and a UserRegistered message published
        using (IServiceScope scope = appFactory.Services.CreateScope())
        {
            ECommerceDbContext db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
            string storedEmail = email.Trim().ToUpperInvariant();
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == storedEmail);

            user.Should().NotBeNull();
            user!.IsEmailVerified.Should().BeTrue();
            user.Status.Should().Be(RegistrationConstants.RegistrationActive);
        }

        // Verify that the verify handler published the UserRegistered message
        appFactory.Publisher.PublishedMessages.Should().Contain(m => m.GetType().Name == "UserRegistered");
    }
}
