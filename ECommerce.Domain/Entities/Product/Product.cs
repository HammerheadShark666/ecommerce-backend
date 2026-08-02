using ECommerce.Domain.Common;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities.Product;

public class Product : AuditableEntity<Guid>
{
    public Guid? CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public string Name { get; set; } = null!;

    public required string Slug { get; set; }

    public string? Description { get; set; }

    public string? ShortDescription { get; set; }

    public Money BasePrice { get; set; } = new Money(0, "GBP");

    public int StockQuantity { get; private set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    public Category? Category { get; set; }

    public Brand? Brand { get; set; }

    public void ChangePrice(Money newPrice) => BasePrice = newPrice;

    public void IncreaseStock(int quantity) => StockQuantity += quantity;

    public void ReduceStock(int quantity)
    {
        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException("Insufficient stock.");
        }

        StockQuantity -= quantity;
    }
}
