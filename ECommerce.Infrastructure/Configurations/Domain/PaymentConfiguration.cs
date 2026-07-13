using ECommerce.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("ECOMMERCE_Payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentMethod).HasMaxLength(50);
        builder.Property(x => x.Provider).HasMaxLength(50);
        builder.Property(x => x.TransactionId).HasMaxLength(255);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => x.TransactionId).IsUnique();

        builder.HasOne(x => x.Order)
               .WithOne(o => o.Payment)
               .HasForeignKey<Payment>(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
