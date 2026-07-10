using ECommerce.Application.Abstractions.Configuration;

namespace ECommerce.Application.Configuration;

public class JwtSettings : IJwtSettings
{
    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty; 

    public string Audience { get; init; } = string.Empty;

    public int ExpiryMinutes { get; init; }

    public int RefreshTokenExpiryDays { get; init; }

    public string KeyId { get; init; } = string.Empty;
}
