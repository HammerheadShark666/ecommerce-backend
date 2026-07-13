using System.ComponentModel.DataAnnotations;

namespace ECommerce.Infrastructure.Configurations;

public sealed class EmailOptions
{
    public const string Section = "Email";

    [Required]
    public required string Endpoint { get; init; }

    [Required]
    public required string SenderAddress { get; init; }
}
