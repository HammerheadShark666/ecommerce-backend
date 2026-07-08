using System.ComponentModel.DataAnnotations;

namespace ECommerce.Infrastructure.Configurations;

public class EncryptionOptions
{
    public const string Section = "Encryption";

    [Required]
    public string OneTimePasswordKey { get; set; } = string.Empty;
}
