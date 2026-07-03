using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Product;

public class Brand : AuditableEntity
{
    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; }
}
