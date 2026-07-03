using ECommerce.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName).HasMaxLength(500);
        builder.Property(x => x.Sku).HasMaxLength(100);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.TotalPrice).HasPrecision(18, 2);

        builder.HasOne(x => x.Order)
               .WithMany(x => x.Items)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
