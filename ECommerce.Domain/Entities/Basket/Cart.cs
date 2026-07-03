using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Basket;

public class Cart : AuditableEntity
{
    public Guid UserId { get; set; }

    private readonly List<CartItem> _items = new();

    public IReadOnlyCollection<CartItem> Items => _items;

    public void AddItem(CartItem item) => _items.Add(item);
}
