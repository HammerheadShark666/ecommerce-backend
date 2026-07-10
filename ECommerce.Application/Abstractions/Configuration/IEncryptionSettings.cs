namespace ECommerce.Application.Abstractions.Configuration;

public interface IEncryptionSettings
{
    string OneTimePasswordKey { get; }
}
