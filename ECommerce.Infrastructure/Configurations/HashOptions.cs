using System.ComponentModel.DataAnnotations;

namespace ECommerce.Infrastructure.Configurations;

public class HashOptions
{
    public const string Section = "Hash";

    [Required]
    public string Secret { get; set; } = string.Empty; 
}

