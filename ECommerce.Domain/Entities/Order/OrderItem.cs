using ECommerce.Domain.Common;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities.Order;

public class OrderItem : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? Sku { get; set; }

    public int Quantity { get; set; }

    public Money UnitPrice { get; set; } = default!;

    public Money TotalPrice { get; set; } = default!;

    public Order Order { get; set; } = null!;

    public Product.Product? Product { get; set; }     
}
