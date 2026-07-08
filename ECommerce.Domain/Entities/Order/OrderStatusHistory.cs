using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Order;

public class OrderStatusHistory : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public Guid? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public Order Order { get; set; } = null!;
}
