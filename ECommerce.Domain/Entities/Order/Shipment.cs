using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Order;

public class Shipment : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }

    public string? TrackingNumber { get; set; }

    public string? ShippingMethod { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public string? Status { get; set; }

    public Order Order { get; set; } = null!;
}
