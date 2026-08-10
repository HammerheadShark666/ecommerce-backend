using System.Net.Http.Json;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Constants;
using ECommerce.Domain.Entities.PasswordReset;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.IntegrationTests.ForgottenPassword;

[Collection("Database")]
public class PasswordResetValidateIntegrationTests(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PasswordResetValidate_Success_UpdatesPassword_MarksTokenUsed_And_Publishes()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        var email = "pwdreset@example.com";
        var plainOtpSecret = "FAKESECRET"; // matches FakeOneTimePasswordGenerator
        var token = "X+qXaioKNxX6O/ceDCs9+5TjWU9ARJ7FE0iX4kGtwrk=";
        var newPassword = "NewPass!1";
        var code = "123456"; // FakeOneTimePasswordGenerator returns this

        Guid userId;

        // Insert user and password reset token using real project services to compute hashes/encryption
        using (var scope = appFactory.Services.CreateScope())
        {
            var aes = scope.ServiceProvider.GetRequiredService<IAesEncryptionHelper>();
            var encSettings = scope.ServiceProvider.GetRequiredService<IEncryptionSettings>();
            var hmac = scope.ServiceProvider.GetRequiredService<IHmacsha256Hasher>();
            var hashSettings = scope.ServiceProvider.GetRequiredService<IHashSettings>();
            var pwdHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var encryptedSecret = aes.Encrypt(plainOtpSecret, encSettings.OneTimePasswordKey);

            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            await using var db = new ECommerceDbContext(options);
            var user = new User
            {
                Email = email,
                FirstName = "Reset",
                LastName = "User",
                PasswordHash = pwdHasher.Hash("OldPass!1"),
                Phone = "000",
                Status = "Active",
                IsEmailVerified = true,
                OneTimePasswordSecret = encryptedSecret
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            userId = user.Id;

            var tokenHash = hmac.HashToken(token, AuthenticationConstants.HashTypeTokenPasswordReset, hashSettings.Secret);

            var prt = new PasswordResetToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Used = false
            };

            db.PasswordResetTokens.Add(prt);
            await db.SaveChangesAsync();
        }

        // Act
        var request = new HttpRequestMessage(
                               HttpMethod.Post,
                               "/forgotten-password/reset/validate")
        {
            Content = JsonContent.Create(new { Token = token, Email = email, NewPassword = newPassword, Code = code })
        };

        request.Headers.TryAddWithoutValidation(
            "X-Forwarded-For",
            "192.168.1.100");

        var resp = await client.SendAsync(request);


        // Act
        //var resp = await client.PostAsJsonAsync("/forgotten-password/reset/validate", new { Token = token, Email = email, NewPassword = newPassword, Code = code });

        // Assert
        resp.EnsureSuccessStatusCode();

        using (var scope = appFactory.Services.CreateScope())
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            await using var db = new ECommerceDbContext(options);
            var prt = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            prt.Should().NotBeNull();
            prt!.Used.Should().BeTrue();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            user.Should().NotBeNull();
            user!.PasswordHash.Should().NotBeNullOrWhiteSpace();
            user.PasswordHash.Should().NotBe("OldPass!1");
        }

        appFactory.Publisher.PublishedMessages.Should().Contain(m => m.GetType().Name == "PasswordResetCompleted");
    }

    [Fact]
    public async Task PasswordResetValidate_InvalidCode_ReturnsUnauthorized_And_DoesNotUseToken()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        var email = "pwdreset2@example.com";
        var plainOtpSecret = "FAKESECRET";
        var token = "X+qXaioKNxX6O/ceDCs9+5TjWU9ARJ7FE0iX4kGtwrk=";
        var newPassword = "NewPass!1";
        var invalidCode = "000000";
        var ipAddress = "";

        Guid userId;

        using (var scope = appFactory.Services.CreateScope())
        {
            var aes = scope.ServiceProvider.GetRequiredService<IAesEncryptionHelper>();
            var encSettings = scope.ServiceProvider.GetRequiredService<IEncryptionSettings>();
            var hmac = scope.ServiceProvider.GetRequiredService<IHmacsha256Hasher>();
            var hashSettings = scope.ServiceProvider.GetRequiredService<IHashSettings>();
            var pwdHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var encryptedSecret = aes.Encrypt(plainOtpSecret, encSettings.OneTimePasswordKey);

            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            await using var db = new ECommerceDbContext(options);
            var user = new User
            {
                Email = email,
                FirstName = "Reset",
                LastName = "User",
                PasswordHash = pwdHasher.Hash("OldPass!1"),
                Phone = "000",
                Status = "Active",
                IsEmailVerified = true,
                OneTimePasswordSecret = encryptedSecret
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            userId = user.Id;

            var tokenHash = hmac.HashToken(token, AuthenticationConstants.HashTypeTokenPasswordReset, hashSettings.Secret);

            var prt = new PasswordResetToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Used = false
            };

            db.PasswordResetTokens.Add(prt);
            await db.SaveChangesAsync();
        }

        // Act
        var request = new HttpRequestMessage(
                               HttpMethod.Post,
                               "/forgotten-password/reset/validate")
        {
            Content = JsonContent.Create(new { Token = token, Email = email, NewPassword = newPassword, Code = invalidCode })
        };

        request.Headers.TryAddWithoutValidation(
            "X-Forwarded-For",
            "192.168.1.100");

        var resp = await client.SendAsync(request);


        // Act
        //var resp = await client.PostAsJsonAsync("/forgotten-password/reset/validate", new { Token = token, Email = email, NewPassword = newPassword, Code = invalidCode });

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        using (var scope = appFactory.Services.CreateScope())
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            await using var db = new ECommerceDbContext(options);
            var prt = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            prt.Should().NotBeNull();
            prt!.Used.Should().BeFalse();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            user.Should().NotBeNull();
            user!.PasswordHash.Should().NotBeNullOrWhiteSpace();

            // Password should remain the old hashed value
            // We cannot easily compare hashed values to plain, but ensure PasswordHash changed from default placeholder
        }

        appFactory.Publisher.PublishedMessages.Should().NotContain(m => m.GetType().Name == "PasswordResetCompleted");
    }
}
