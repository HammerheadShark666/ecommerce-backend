using ECommerce.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);

        builder.HasOne<Product>()
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
