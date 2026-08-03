using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Library.Authentication;
using ECommerce.IntegrationTests.Fakes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.UnitTests.Infracstructure.Library;

public class JwtGeneratorTest
{
    // ----------------------------------------------------------------
    //  Shared helpers
    // ----------------------------------------------------------------

    /// <summary>Minimum valid secret – exactly 32 bytes (256 bits) for HMAC-SHA256.</summary>
    private const string ValidSecret = "lsdf09dsjf09dsflds0df##324kjdsf(*&KJHG3324sfdsdsf";
    private const string ValidIssuer = "https://issuer.example.com";
    private const string ValidAudience = "https://audience.example.com";
    private const int ValidExpiryMinutes = 15;

    private static JwtGenerator CreateSut(params string[] roles) =>
        new(new FakeUserClaimsFactory(roles), GetJwtSettings());

    private static JwtGenerator CreateSut(OptionsWrapper<JwtOptions> settings, params string[] roles) =>
        new(new FakeUserClaimsFactory(roles), settings);

    // ================================================================
    //  1. HAPPY PATH TESTS
    // ================================================================

    [Fact]
    public async Task GenerateToken_WithValidInputs_ReturnsNonEmptyString()
    {
        // Arrange  
        var jwtGenerator = CreateSut();

        // Act
        var token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }     

    [Fact]
    public async Task GenerateToken_WithValidInputs_ReturnsWellFormedJwt()
    {
        // Arrange
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        // A valid JWT has exactly three Base64url segments separated by dots
        string[] parts = token.Split('.');

        // Assert
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task GenerateToken_WithValidInputs_TokenPassesSignatureValidation()
    {
        // Arrange
        var jwtGenerator = CreateSut();
        User user = CreateUser();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);

        // Assert
        Assert.NotNull(parsed);
    }

    [Fact]
    public async Task GenerateToken_ClaimsContainCorrectSubject()
    {
        // Arrange
        User user = CreateUser();
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        Claim sub = parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub);

        // Assert
        Assert.Equal(user.Id.ToString(), sub.Value);
    }

    [Fact]
    public async Task GenerateToken_ClaimsContainCorrectEmail()
    {
        // Arrange
        User user = CreateUser();
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        Claim email = parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email);

        // Assert
        Assert.Equal(user.Email, email.Value);
    }

    [Fact]
    public async Task GenerateToken_ClaimsContainJti()
    {
        // Arrange
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        Claim? jti = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        // Assert
        Assert.NotNull(jti);
        Assert.True(Guid.TryParse(jti.Value, out _), "jti must be a valid GUID");
    }

    [Fact]
    public async Task GenerateToken_TokenExpiresApproximatelyFifteenMinutesFromNow()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        DateTime after = DateTime.UtcNow;
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        DateTime expires = parsed.ValidTo;

        // Assert
        Assert.InRange(expires,
            before.AddMinutes(15).AddSeconds(-5),
            after.AddMinutes(15).AddSeconds(5));
    }

    [Fact]
    public async Task GenerateToken_IssuerMatchesConfig()
    {
        // Arrange
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);

        // Assert
        Assert.Equal(ValidIssuer, parsed.Issuer);
    }

    [Fact]
    public async Task GenerateToken_AudienceMatchesConfig()
    {
        // Arrange
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);

        // Assert
        Assert.Equal(ValidAudience, parsed.Audiences.First());
    }

    [Fact]
    public async Task GenerateToken_UsesHmacSha256Algorithm()
    {
        // Act
        var jwtGenerator = CreateSut();
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken parsed = handler.ReadJwtToken(token);

        // Assert
        Assert.Equal(SecurityAlgorithms.HmacSha256, parsed.Header.Alg);
    }

    // ================================================================
    //  2. JTI UNIQUENESS
    // ================================================================

    [Fact]
    public async Task GenerateToken_TwoCallsForSameUser_ProduceDifferentJtis()
    {
        // Arrange
        User user = CreateUser();
        var jwtGenerator = CreateSut();

        // Act
        string token1 = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        string token2 = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jti1 = handler.ReadJwtToken(token1);
        JwtSecurityToken jti2 = handler.ReadJwtToken(token2);

        // Assert
        Assert.NotEqual(jti1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value, jti2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    // ================================================================
    //  3. CONFIGURATION FALLBACK TESTS
    // ================================================================

    [Fact]
    public async Task GenerateToken_WhenIssuerMissing_FallsBackToProjectName()
    {
        // Arrange
        // Omit issuer from config – expect Constants.ProjectName as fallback 

        var jwtGenerator = CreateSut(GetJwtSettings(issuer: null)); 
        var handler = new JwtSecurityTokenHandler();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        JwtSecurityToken parsed = handler.ReadJwtToken(token);

        // Assert
        Assert.Equal(ValidIssuer, parsed.Issuer);
    }

    [Fact]
    public async Task GenerateToken_WhenAudienceMissing_FallsBackToProjectName()
    {
        // Arrange 
        var jwtGenerator = CreateSut(GetJwtSettings(audience: null));
        var token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);

        // Act
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken parsed = handler.ReadJwtToken(token);

        // Assert
        Assert.Equal(ValidAudience, parsed.Audiences.First());
    }

    // ================================================================
    //  4. EXCEPTION / ERROR-HANDLING TESTS
    // ================================================================

    [Fact]
    public async Task GenerateToken_WhenSecretMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var jwtGenerator = CreateSut(GetJwtSettingsSetSecretEmpty()); 

        // Act & Assert
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(
            async () => await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None));
    }

    [Fact]
    public async Task GenerateToken_WhenUserIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var jwtGenerator = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () => await jwtGenerator.GenerateTokenAsync(null!, CancellationToken.None));
    }

    // ================================================================
    //  5. BOUNDARY VALUE TESTS – SECRET LENGTH
    // ================================================================

    [Fact]
    public async Task GenerateToken_WithExactly32ByteSecret_Succeeds()
    {
        // Arrange
        // 32 bytes = minimum recommended for HMAC-SHA256
        string secret = new('x', 32); 
        var jwtGenerator = CreateSut(GetJwtSettings(audience: null));

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None); // must not throw

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task GenerateToken_WithVeryLongSecret_Succeeds()
    {
        // Arrange
        string secret = new('x', 512); 
        var jwtGenerator = CreateSut(GetJwtSettings(audience: null));

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task GenerateToken_WithShortSecret_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        // Keys shorter than 128 bits are rejected by IdentityModel  
        var jwtGenerator = CreateSut(GetJwtSettings(secret: "rdf"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None));
    }

    // ================================================================
    //  6. BOUNDARY VALUE TESTS – USER FIELDS
    // ================================================================

    [Fact]
    public async Task GenerateToken_WithEmptyEmail_TokenContainsEmptyEmailClaim()
    {
        // Arrange
        User user = CreateUser("");
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        Claim email = parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email);

        // Assert
        Assert.Equal("", email.Value);
    }

    [Fact]
    public async Task GenerateToken_WithVeryLongEmail_TokenContainsFullEmail()
    {
        // Arrange
        string longEmail = new string('a', 200) + "@example.com";
        User user = CreateUser(longEmail);
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        Claim email = parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email);

        // Assert
        Assert.Equal(longEmail, email.Value);
    }

    [Fact]
    public async Task GenerateToken_WithSpecialCharactersInUsername_EncodesCorrectly()
    {
        User user = CreateUser("a@b.com");
        var jwtGenerator = CreateSut();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(user, CancellationToken.None);
        JwtSecurityToken parsed = ParseToken(token, ValidSecret, ValidIssuer, ValidAudience);
        Claim email = parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email);

        // Assert
        Assert.Equal(user.Email, email.Value);
    }

    // ================================================================
    //  7. TOKEN INTEGRITY TESTS
    // ================================================================

    [Fact]
    public async Task GenerateToken_TamperedToken_FailsValidation()
    {
        // Arrange
        var jwtGenerator = CreateSut();
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);

        // Flip one character in the signature segment
        string[] parts = token.Split('.');
        string sig = parts[2];
        parts[2] = (sig[0] == 'A' ? "B" : "A") + sig[1..];
        string tampered = string.Join('.', parts);

        // Act & Assert
        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(
            () => ParseToken(tampered, ValidSecret, ValidIssuer, ValidAudience));
    }

    [Fact]
    public async Task GenerateToken_TokenSignedWithDifferentKey_FailsValidation()
    {
        // Arrange
        var jwtGenerator = CreateSut();
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);

        string wrongSecret = new('z', 32);

        // Act & Assert
        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(
            () => ParseToken(token, wrongSecret, ValidIssuer, ValidAudience));
    }

    // ================================================================
    //  8. CLAIMS COUNT
    // ================================================================

    [Fact]
    public async Task GenerateToken_TokenContainsExactlyFourCustomClaims()
    {
        // Arrange
        var jwtGenerator = CreateSut();
        var handler = new JwtSecurityTokenHandler();

        // Act
        string token = await jwtGenerator.GenerateTokenAsync(CreateUser(), CancellationToken.None);
        JwtSecurityToken parsed = handler.ReadJwtToken(token);

        string[] customTypes =
        [
            JwtRegisteredClaimNames.Sub,
            JwtRegisteredClaimNames.Email,
            JwtRegisteredClaimNames.Jti
        ];

        // Assert
        foreach (string type in customTypes)
        {
            Assert.Single(parsed.Claims, c => c.Type == type);
        }
    }

    private static OptionsWrapper<JwtOptions> GetJwtSettings(
                                                    string? secret = null,
                                                    string? issuer = null,
                                                    string? audience = null,
                                                    int? expiryMinutes = null)
    {
        JwtOptions options = new()
        {
            Secret = secret ?? ValidSecret,
            Issuer = issuer ?? ValidIssuer,
            Audience = audience ?? ValidAudience,
            ExpiryMinutes = expiryMinutes ?? ValidExpiryMinutes
        };

        return new OptionsWrapper<JwtOptions>(options);
    }

    private static OptionsWrapper<JwtOptions> GetJwtSettingsSetSecretEmpty()
    {
        JwtOptions options = new()
        {
            Secret = ""
        };

        return new OptionsWrapper<JwtOptions>(options);
    }

    private static User CreateUser(
                    string email = "a@b.com",
                    string firstName = "",
                    string lastName = "",
                    string status = "Active") => new()
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        Status = status,
                        LastName = lastName,
                        FirstName = firstName,
                        PasswordHash = string.Empty,
                        Phone = string.Empty
                    };


    /// <summary>Parse and validate a JWT, returning the handler + principal.</summary>
    private static JwtSecurityToken ParseToken(string jwt, string secret,
        string issuer, string audience)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        handler.ValidateToken(jwt, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => [key],
            TryAllIssuerSigningKeys = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken? validated);

        return (JwtSecurityToken)validated;
    }
}
