using System.Net.Http.Json;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using ECommerce.IntegrationTests.Library.Intefaces;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.IntegrationTests.TwoFactorEnrolment;

[Collection("Database")]
public class TwoFactorEnrolmentIntegrationTest : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly TestApplicationFactory _appFactory;
    private readonly HttpClient _client;

    public TwoFactorEnrolmentIntegrationTest(SqlServerFixture fixture)
    {
        _fixture = fixture;
        _appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        _client = _appFactory.CreateClient();
    }

    [Fact]
    public async Task EndToEnd_TwoFactorEnrolment_Successful()
    {
        // Arrange
        const string email = "enrol@example.com";
        const string password = "EnrolPass!1";

        using (var scope = _appFactory.Services.CreateScope())
        {
            var databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            await databaseHelper.SeedUserAsync(_fixture, email, password, isTwoFactor: false);
        }

        // Act: begin enrol
        var beginResp = await _client.PostAsync($"/2fa/enrol?email={email}", null);
        beginResp.EnsureSuccessStatusCode();

        var beginDto = await beginResp.Content.ReadFromJsonAsync<BeginEnrolResponse>();
        beginDto.Should().NotBeNull();
        beginDto!.OtpAuthUri.Should().Contain("secret=");

        var queryParams = QueryHelpers.ParseQuery(new Uri(beginDto.OtpAuthUri).Query);
        var secret = queryParams["secret"].First() 
            ?? throw new InvalidOperationException("Failed to parse query string.");
        secret.Should().NotBeNull();

        var oneTimeCode = GetOneTimeCode(secret);

        var confirmResp = await _client.PostAsJsonAsync("/2fa/enrol/confirm", new { Email = email, Code = oneTimeCode });
        confirmResp.EnsureSuccessStatusCode();

        // Assert response
        var confirmDto = await confirmResp.Content.ReadFromJsonAsync<ConfirmTwoFactorEnrolmentResponse>();
        confirmDto.Should().NotBeNull();
        confirmDto.Message.Should().Be("2FA enabled successfully.");


        // Assert DB updated
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var db = new ECommerceDbContext(options);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        user.Should().NotBeNull();
        user!.IsTwoFactorEnabled.Should().BeTrue();
        user.OneTimePasswordSecret.Should().NotBeNullOrWhiteSpace();
    }

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record BeginEnrolResponse(string QrCodeBase64, string OtpAuthUri);
    private record ConfirmTwoFactorEnrolmentResponse(string Message);

    private string GetOneTimeCode(string secret)
    {
        using var scope = _appFactory.Services.CreateScope();
        var totpGenerator = scope.ServiceProvider.GetRequiredService<IOneTimePasswordGenerator>();
        return totpGenerator.GetCurrentCode(secret);
    }
}
