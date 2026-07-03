using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Order;

public class OrderItem : AuditableEntity
{
    public Guid OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    public string? ProductName { get; set; }

    public string? Sku { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Order Order { get; set; } = null!;

    public Product.Product? Product { get; set; } 
}
