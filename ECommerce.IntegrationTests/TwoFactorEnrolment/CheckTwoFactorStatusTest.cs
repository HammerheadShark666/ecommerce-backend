using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Features.Security.CheckTwoFactorStatus;
using ECommerce.IntegrationTests.Library;
using ECommerce.IntegrationTests.Library.Intefaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.IntegrationTests.TwoFactorEnrolment;

[Collection("Database")]
public class CheckTwoFactorStatusTest : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture;
    private readonly TestApplicationFactory _appFactory;
    private readonly HttpClient _client;

    public CheckTwoFactorStatusTest(SqlServerFixture fixture)
    {
        _fixture = fixture;         
        _appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        _client = _appFactory.CreateClient();
    }

    [Fact]
    public async Task GetStatus_ReturnsTwoFactorStatus()
    {
        // Arrange
        var email = "status-enabled@example.com";

        using (var scope = _appFactory.Services.CreateScope())
        {
            var databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            var user = await databaseHelper.SeedUserAsync(_fixture, email, "P@ssw0rd!", isTwoFactor: true);
        }

        // Act
        var response = await _client.GetAsync($"/2fa/status?email={email}"); 

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<GetTwoFactorStatusResponse>();

        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatus_ReturnsNotFoundException()
    {
        // Arrange
        var notFoundEmail = "does-not-exist@example.com";

        // Act
        var response = await _client.GetAsync($"/2fa/status?email={notFoundEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);       
    }

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask; 
}
