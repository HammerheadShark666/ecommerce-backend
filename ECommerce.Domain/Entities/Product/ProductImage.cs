using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Product;

public class ProductImage : AuditableEntity
{
    public Guid ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}
