using System.ComponentModel.DataAnnotations;

namespace ECommerce.Infrastructure.Configurations;

public sealed class UrlOptions
{
    public const string Section = "Url";

    [Required]
    public required string FrontEnd { get; init; }
}
