using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Order;

public class Payment : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Provider { get; set; }

    public string? TransactionId { get; set; }

    public string? Status { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public Order Order { get; set; } = null!;
}
