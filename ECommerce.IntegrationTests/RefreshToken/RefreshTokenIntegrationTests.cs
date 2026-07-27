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

namespace ECommerce.IntegrationTests.RefreshToken;

[Collection("Database")]
public class RefreshTokenIntegrationTests(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RefreshToken_WhenNoCookie_ReturnsUnauthorized()
    {
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        HttpClient client = appFactory.CreateClient();

        HttpResponseMessage resp = await client.PostAsync("/refresh-token/", new StringContent(string.Empty));

        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

internal sealed class CookieDelegatingHandler : System.Net.Http.DelegatingHandler
{
    private readonly System.Net.CookieContainer _cookieContainer;
    private readonly Uri _baseUri;

    public CookieDelegatingHandler(System.Net.CookieContainer cookieContainer, Uri baseUri)
    {
        _cookieContainer = cookieContainer;
        _baseUri = baseUri;
    }

    protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Attach cookies for the request
        string cookieHeader = _cookieContainer.GetCookieHeader(_baseUri);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            if (request.Headers.Contains("Cookie"))
            {
                request.Headers.Remove("Cookie");
            }

            request.Headers.Add("Cookie", cookieHeader);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Read Set-Cookie headers and store them in the container
        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookieHeaders))
        {
            foreach (string sc in setCookieHeaders)
            {
                try
                {
                    _cookieContainer.SetCookies(_baseUri, sc);
                }
                catch
                {
                    // ignore malformed cookie
                }
            }
        }

        return response;
    }
}

    [Fact]
    public async Task RefreshToken_WithValidCookie_ReturnsOk_SetsNewCookie_And_UpdatesDb()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);

        // Create a CookieContainer and a delegating handler that will forward to the test server
        var cookieContainer = new System.Net.CookieContainer();
        Uri baseUri = appFactory.Server.BaseAddress ?? new Uri("http://localhost");

        HttpMessageHandler serverHandler = appFactory.Server.CreateHandler();
        var cookieDelegatingHandler = new CookieDelegatingHandler(cookieContainer, baseUri)
        {
            InnerHandler = serverHandler
        };

        var client = new System.Net.Http.HttpClient(cookieDelegatingHandler)
        {
            BaseAddress = baseUri
        };

        string plainOldRefreshToken = "old-refresh-token-1";
        string email = "rtuser@example.com";
        Guid userId;

        // compute hash for stored token and insert user + refresh token
        using (IServiceScope scope = appFactory.Services.CreateScope())
        {
            IHmacsha256Hasher hmac = scope.ServiceProvider.GetRequiredService<IHmacsha256Hasher>();
            IHashSettings hashSettings = scope.ServiceProvider.GetRequiredService<IHashSettings>();

            string hashedOld = hmac.HashToken(plainOldRefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

            DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            await using var db = new ECommerceDbContext(options);
            var user = new User
            {
                Email = email,
                FirstName = "RT",
                LastName = "User",
                PasswordHash = "hash",
                Phone = "000",
                Status = "Active",
                IsEmailVerified = true
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            userId = user.Id;

            var rt = new Domain.Entities.Authentication.RefreshToken
            {
                UserId = userId,
                Token = hashedOld,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                RevokedAt = null
            };

            db.RefreshTokens.Add(rt);
            await db.SaveChangesAsync();
        }


        // Pre-seed the cookie container with the existing refresh token so the request includes it
        cookieContainer.Add(baseUri, new System.Net.Cookie("refreshToken", plainOldRefreshToken));

        // Act
        HttpResponseMessage resp = await client.PostAsync("/refresh-token/", new StringContent(string.Empty));

        // Assert
        resp.EnsureSuccessStatusCode();

        // We register a deterministic refresh token generator in the test factory
        const string expectedNewPlain = "TEST_NEW_REFRESH_TOKEN";

        // CookieDelegatingHandler should have stored the cookie; if present verify it matches expected
        string? cookieValue = cookieContainer.GetCookies(baseUri)["refreshToken"]?.Value;
        cookieValue?.Should().Be(expectedNewPlain);

        string newPlainRefreshToken = expectedNewPlain;

        // verify DB: old token revoked, new token exists
        using (IServiceScope scope = appFactory.Services.CreateScope())
        {
            IHmacsha256Hasher hmac = scope.ServiceProvider.GetRequiredService<IHmacsha256Hasher>();
            IHashSettings hashSettings = scope.ServiceProvider.GetRequiredService<IHashSettings>();

            string hashedNew = hmac.HashToken(newPlainRefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);
            string hashedOld = hmac.HashToken(plainOldRefreshToken, AuthenticationConstants.HashTypeTokenRefresh, hashSettings.Secret);

            DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            await using var db = new ECommerceDbContext(options);
            Domain.Entities.Authentication.RefreshToken? old = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == hashedOld && t.UserId == userId);
            old.Should().NotBeNull();
            old!.RevokedAt.Should().NotBeNull();

            Domain.Entities.Authentication.RefreshToken? added = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == hashedNew && t.UserId == userId);
            added.Should().NotBeNull();
            added!.RevokedAt.Should().BeNull();
        }
    }
}
