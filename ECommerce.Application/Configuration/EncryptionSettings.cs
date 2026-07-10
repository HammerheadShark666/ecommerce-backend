using ECommerce.Application.Abstractions.Configuration;

namespace ECommerce.Application.Configuration;

public class EncryptionSettings : IEncryptionSettings
{
    public string OneTimePasswordKey { get; init; } = string.Empty;
}
