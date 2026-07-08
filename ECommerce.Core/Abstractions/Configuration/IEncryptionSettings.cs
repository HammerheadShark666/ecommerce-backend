namespace ECommerce.Core.Abstractions.Configuration;

public interface IEncryptionSettings
{
    string OneTimePasswordKey { get; }
}
