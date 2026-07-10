using System.Net.Http.Json;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using ECommerce.IntegrationTests.Library.Intefaces;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
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

        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            await databaseHelper.SeedUserAsync(_fixture, email, password, isTwoFactor: false);
        }

        // Act: begin enrol
        HttpResponseMessage beginResp = await _client.PostAsync($"/2fa/enrol?email={email}", null);
        beginResp.EnsureSuccessStatusCode();

        BeginEnrolResponse? beginDto = await beginResp.Content.ReadFromJsonAsync<BeginEnrolResponse>();
        beginDto.Should().NotBeNull();
        beginDto!.OtpAuthUri.Should().Contain("secret=");

        Dictionary<string, StringValues> queryParams = QueryHelpers.ParseQuery(new Uri(beginDto.OtpAuthUri).Query);
        string secret = queryParams["secret"].First() 
            ?? throw new InvalidOperationException("Failed to parse query string.");
        secret.Should().NotBeNull();

        string oneTimeCode = GetOneTimeCode(secret);

        HttpResponseMessage confirmResp = await _client.PostAsJsonAsync("/2fa/enrol/confirm", new { Email = email, Code = oneTimeCode });
        confirmResp.EnsureSuccessStatusCode();

        // Assert response
        ConfirmEnrolResponse? confirmDto = await confirmResp.Content.ReadFromJsonAsync<ConfirmEnrolResponse>();
        confirmDto.Should().NotBeNull();
        confirmDto!.Success.Should().BeTrue();

        // Assert DB updated
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var db = new ECommerceDbContext(options);
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        user.Should().NotBeNull();
        user!.IsTwoFactorEnabled.Should().BeTrue();
        user.OneTimePasswordSecret.Should().NotBeNullOrWhiteSpace();
    }

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record BeginEnrolResponse(string QrCodeBase64, string OtpAuthUri);
    private record ConfirmEnrolResponse(bool Success, string Message);

    private string GetOneTimeCode(string secret)
    {
        using IServiceScope scope = _appFactory.Services.CreateScope();
        IOneTimePasswordGenerator totpGenerator = scope.ServiceProvider.GetRequiredService<IOneTimePasswordGenerator>();
        return totpGenerator.GetCurrentCode(secret);
    }
}
