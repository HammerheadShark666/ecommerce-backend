using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Product;

public class Product : AuditableEntity
{
    public Guid CategoryId { get; set; }

    public Guid BrandId { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public string? ShortDescription { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }
}
