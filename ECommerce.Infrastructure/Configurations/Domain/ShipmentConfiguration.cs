using ECommerce.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("ECOMMERCE_Shipments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TrackingNumber).HasMaxLength(200);
        builder.Property(x => x.ShippingMethod).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(50);

        builder.HasOne(x => x.Order)
               .WithOne(x => x.Shipment)
               .HasForeignKey<Shipment>(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderId).IsUnique();
    }
}
