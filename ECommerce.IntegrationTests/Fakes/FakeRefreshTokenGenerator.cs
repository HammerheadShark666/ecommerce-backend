using ECommerce.Application.Abstractions;

namespace ECommerce.IntegrationTests.Fakes;

public sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly string _fixed = "TEST_NEW_REFRESH_TOKEN";

    public string GenerateRefreshToken() => _fixed;
}
