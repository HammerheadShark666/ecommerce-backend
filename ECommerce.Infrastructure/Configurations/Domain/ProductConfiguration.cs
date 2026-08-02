using ECommerce.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("ECOMMERCE_Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ShortDescription).HasMaxLength(500);
         
        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasOne(x => x.Category)
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Brand)
                .WithMany()
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);         

        builder.OwnsOne(x => x.BasePrice, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2);

            money.Property(x => x.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });
    }
}
