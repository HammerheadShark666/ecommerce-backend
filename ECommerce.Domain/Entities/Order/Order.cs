using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Order;

public class Order : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }

    public Guid? ShippingAddressId { get; set; }

    public Guid? BillingAddressId { get; set; }

    public string? OrderNumber { get; set; }

    public DateTime OrderDate { get; set; }

    public string? Status { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal TotalAmount { get; set; }

    private readonly List<OrderItem> _items = [];

    public IReadOnlyCollection<OrderItem> Items => _items;

    private readonly List<OrderStatusHistory> _statusHistory = [];

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory;

    public Shipment? Shipment { get; set; }

    public Payment? Payment { get; set; }

    public void AddItem(OrderItem item) => _items.Add(item);

    public void AddStatus(OrderStatusHistory status) => _statusHistory.Add(status);
}
