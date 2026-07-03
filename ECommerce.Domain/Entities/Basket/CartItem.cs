using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Basket;

public class CartItem : AuditableEntity
{
    public Guid CartId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public Cart Cart { get; set; } = null!;

    public Product.Product? Product { get; set; }
}
