namespace ECommerce.Application.Abstractions.Configuration;

public interface IJwtSettings
{
    string Issuer { get; }

    string Audience { get; }

    string Secret { get; }
    int ExpiryMinutes { get; }

    int RefreshTokenExpiryDays { get; }

    string KeyId { get; }
}
