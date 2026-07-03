using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Product;

public class Category : AuditableEntity
{
    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}
