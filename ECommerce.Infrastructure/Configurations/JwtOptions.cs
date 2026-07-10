using System.ComponentModel.DataAnnotations;

namespace ECommerce.Infrastructure.Configurations;

public class JwtOptions
{
    public const string Section = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32, ErrorMessage = "Jwt:SecretKey must be at least 32 characters")]
    public string Secret { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 15;

    [Range(1, 90)]
    public int RefreshTokenExpiryDays { get; set; } = 7;

    [Required]
    public string KeyId { get; set; } = string.Empty;
}
