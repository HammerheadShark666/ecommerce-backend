using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Basket;

public class Cart : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }

    private readonly List<CartItem> _items = [];

    public IReadOnlyCollection<CartItem> Items => _items;

    public void AddItem(CartItem item) => _items.Add(item);
}
