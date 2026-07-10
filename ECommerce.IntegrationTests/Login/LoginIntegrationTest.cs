using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities.Authentication;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using ECommerce.IntegrationTests.Library.Intefaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ECommerce.IntegrationTests.Login;

[Collection("Database")]
public class LoginIntegrationTest : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly TestApplicationFactory _appFactory;
    private readonly HttpClient _client; 
    
    public LoginIntegrationTest(SqlServerFixture fixture)
    {       
        _fixture = fixture;
        _appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        _client = _appFactory.CreateClient();
    }

    //Tests

    [Fact]
    public async Task EndToEnd_Login_Then_Verify_2Fa_Allows_Protected_Access()
    {
        //Arrange
        const string email = "e2e2fa@example.com";
        const string password = "E2ePass!23";
        string oneTimePasswordCode = "";

        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IAesEncryptionHelper aesEncryptionHelper = scope.ServiceProvider.GetRequiredService<IAesEncryptionHelper>();
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            IOneTimePasswordGenerator oneTimePasswordGenerator = scope.ServiceProvider.GetRequiredService<IOneTimePasswordGenerator>();

            string oneTimePasswordSecret = oneTimePasswordGenerator.GenerateSecret();

            IOptions<EncryptionOptions> options = scope.ServiceProvider.GetRequiredService<IOptions<EncryptionOptions>>();
            EncryptionOptions encryptionSettings = options.Value;

            string encryptionKey = encryptionSettings.OneTimePasswordKey;
            string encryptedOneTimePasswordSecret = aesEncryptionHelper.Encrypt(oneTimePasswordSecret, encryptionKey);

            User user = await SeedTwoFactorUserAsync(scope, email, password, encryptedOneTimePasswordSecret);
            oneTimePasswordCode = ResolveOneTimePasswordCode(scope, encryptedOneTimePasswordSecret);
        }

        //Act
        LoginResponseDto loginDto = await LoginAsync(email, password);
        VerifyResponseDto verifyDto = await Verify2FaAsync(email, loginDto.PendingToken!, oneTimePasswordCode, loginDto.PendingTokenId);
        HttpResponseMessage protectedResp = await CallProtectedEndpointAsync(verifyDto.Token!);
          
        //Assert
        protectedResp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Jwt_Expired_IsRejected()
    {
        //Arrange
        IOptions<JwtOptions> options;

        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            options = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>();
        }

        string secret = options.Value.Secret;
        string issuer = options.Value.Issuer;
        string audience = options.Value.Audience;

        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: [],
            expires: DateTime.UtcNow.AddMinutes(-10), // already expired (beyond default clock skew)
            signingCredentials: creds);

        string expiredToken = new JwtSecurityTokenHandler().WriteToken(token);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        HttpResponseMessage resp = await _client.GetAsync("/protected/me");

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Jwt_Tampered_IsRejected()
    {
        IJwtGenerator jwtGen;
        string token;

        //Arrange
        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            jwtGen = scope.ServiceProvider.GetRequiredService<ECommerce.Application.Abstractions.IJwtGenerator>();
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            User user = await databaseHelper.SeedUserAsync(_fixture, "jwt@example.com", "JwtPass1!", isTwoFactor: false);
            token = await jwtGen.GenerateTokenAsync(user);
        }

        string tamperedJwtToken = CreateTamperedJwtToken(token);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedJwtToken);

        // Act
        HttpResponseMessage resp = await _client.GetAsync("/protected/me");

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Returns_Jwt_When_TwoFactor_Disabled()
    {
        //Arrange
        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            User user = await databaseHelper.SeedUserAsync(_fixture, "no2fa@example.com", "P@ssw0rd!", isTwoFactor: false);
        }

        // Act
        LoginResponseDto? dto = await PostLoginAndParseAsync("no2fa@example.com", "P@ssw0rd!");

        // Assert
        dto.Should().NotBeNull();
        dto!.RequiresTwoFactor.Should().BeFalse();
        dto.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Returns_PendingToken_When_TwoFactor_Enabled()
    {
        //Arrange
        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            User user = await databaseHelper.SeedUserAsync(_fixture, "with2fa@example.com", "Secret123!", isTwoFactor: true, oneTimePasswordSecret: "JBSWY3DPEHPK3PXP");
        }

        // Act
        LoginResponseDto? dto = await PostLoginAndParseAsync("with2fa@example.com", "Secret123!");

        // Assert
        dto.Should().NotBeNull();
        dto!.RequiresTwoFactor.Should().BeTrue();
        dto.PendingToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Fails_With_Invalid_Username()
    {
        // Act  
        HttpResponseMessage resp = await PostLoginRawAsync("doesnotexist@example.com", "whatever");

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Login_Fails_With_Invalid_Password()
    {
        //Arrange
        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            User user = await databaseHelper.SeedUserAsync(_fixture, "badpass@example.com", "RightPass1!", isTwoFactor: false);
        }

        // Act
        HttpResponseMessage resp = await PostLoginRawAsync("badpass@example.com", "WrongPass!");

        // Assert
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PendingTokenExpiry_IsRejected_And_Cleared()
    {
        // Arrange
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        string stored = Convert.ToBase64String(hash);
        string email = "expired@example.com";
        string password = "Expired1!";
        string secret = "JBSWY3DPEHPK3PXP";
        DateTime expiry = DateTime.UtcNow.AddSeconds(-10); // already expired

        IDatabaseHelper databaseHelper;
        DbContextOptions<ECommerceDbContext> options;
        User user;
        string oneTimeCode;

        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            user = await databaseHelper.SeedUserAsync(_fixture, email, password, isTwoFactor: true, oneTimePasswordSecret: secret);
            options = databaseHelper.CreateOptions(_fixture); // moved inside
        }

        await using (var db = new ECommerceDbContext(options))
        {
            var pendingTwoFactorLogin = new PendingTwoFactorLogin
            {
                UserId = user.Id,
                PendingTwoFactorToken = stored,
                PendingTokenExpiresAt = expiry
            };
            db.PendingTwoFactorLogins.Add(pendingTwoFactorLogin);
            await db.SaveChangesAsync();
        }

        // get one time code
        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IOneTimePasswordGenerator oneTimePasswordGenerator = scope.ServiceProvider.GetRequiredService<IOneTimePasswordGenerator>();
            oneTimeCode = oneTimePasswordGenerator.GetCurrentCode(secret);
        }

        // Act
        HttpResponseMessage verifyResp = await _client.PostAsJsonAsync("/login/2fa/verify", new { Email = email, PendingToken = token, Code = oneTimeCode });

        // Assert
        verifyResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        await using (var db = new ECommerceDbContext(options))
        {
            PendingTwoFactorLogin refreshed = db.PendingTwoFactorLogins.First(u => u.UserId == user.Id);

            string hashedToken = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            refreshed.PendingTwoFactorToken.Should().Be(hashedToken);
            refreshed.PendingTokenExpiresAt.Should().Be(expiry);
        }
    }

    //Functions/Records/Variables     

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record VerifyResponseDto(string? Token); //bool Success,, string Message
    private record LoginResponseDto(bool RequiresTwoFactor, string? PendingToken, string? Token, Guid? PendingTokenId);  
    
    private Task<HttpResponseMessage> PostLoginRawAsync(string email, string password)
        => _client.PostAsJsonAsync("/login", new { Email = email, Password = password });

    private async Task<LoginResponseDto?> PostLoginAndParseAsync(string email, string password)
    {
        HttpResponseMessage resp = await PostLoginRawAsync(email, password);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        return await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
    }


    private static string ResolveOneTimePasswordCode(IServiceScope scope, string oneTimePasswordSecret)
    {
        IOneTimePasswordGenerator oneTimePasswordGenerator = scope.ServiceProvider.GetRequiredService<ECommerce.Application.Abstractions.IOneTimePasswordGenerator>();
        return oneTimePasswordGenerator.GetCurrentCode(oneTimePasswordSecret);
    }

    private async Task<User> SeedTwoFactorUserAsync(IServiceScope scope, string email, string password, string oneTimePasswordSecret)
    {
        IDatabaseHelper db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
        return await db.SeedUserAsync(_fixture, email, password, isTwoFactor: true, oneTimePasswordSecret: oneTimePasswordSecret);
    }

    private async Task<LoginResponseDto> LoginAsync(string email, string password)
    {
        LoginResponseDto? dto = await PostLoginAndParseAsync(email, password);
        dto.Should().NotBeNull();
        dto!.RequiresTwoFactor.Should().BeTrue();
        dto.PendingToken.Should().NotBeNullOrWhiteSpace();
        return dto;
    }

    private async Task<VerifyResponseDto> Verify2FaAsync(string email, string pendingToken, string totpCode, Guid? pendingTokenId)
    {       
        HttpResponseMessage resp = await _client.PostAsJsonAsync("/login/2fa/verify", new { Email = email, PendingToken = pendingToken, Code = totpCode, PendingTokenId = pendingTokenId });
        resp.EnsureSuccessStatusCode();

        VerifyResponseDto? dto = await resp.Content.ReadFromJsonAsync<VerifyResponseDto>();
        dto.Should().NotBeNull();
        dto.Token.Should().NotBeNullOrWhiteSpace();
        return dto!;
    }

    private async Task<HttpResponseMessage> CallProtectedEndpointAsync(string jwt)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/protected/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        HttpResponseMessage resp = await _client.SendAsync(request);

        return resp;
    }

    private static string PadBase64(string base64)
    {
        int pad = 4 - base64.Length % 4;
        if (pad == 4)
        {
            pad = 0;
        }

        return base64 + new string('=', pad);
    }


    private static string CreateTamperedJwtToken(string token)
    {
        string[] parts = token.Split('.');
        string payload = parts[1];
        byte[] bytes = System.Convert.FromBase64String(PadBase64(payload));
        bytes[0] ^= 0x01; // flip a bit
        parts[1] = System.Convert.ToBase64String(bytes).TrimEnd('=');
        return string.Join('.', parts);
    }

}


