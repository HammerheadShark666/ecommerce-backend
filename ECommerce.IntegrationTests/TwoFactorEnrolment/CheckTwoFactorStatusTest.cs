using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Features.CheckTwoFactorStatus;
using ECommerce.Domain.Entities.User;
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
        string email = "status-enabled@example.com";

        using (IServiceScope scope = _appFactory.Services.CreateScope())
        {
            IDatabaseHelper databaseHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
            User user = await databaseHelper.SeedUserAsync(_fixture, email, "P@ssw0rd!", isTwoFactor: true);
        }

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/2fa/status?email={email}"); 

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        GetTwoFactorStatusResponse? result =
            await response.Content
                .ReadFromJsonAsync<GetTwoFactorStatusResponse>();

        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatus_ReturnsNotFoundException()
    {
        // Arrange
        string notFoundEmail = "does-not-exist@example.com";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/2fa/status?email={notFoundEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);       
    }

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask; 
}
