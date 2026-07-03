using ECommerce.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Slug).HasMaxLength(255);
        builder.Property(x => x.ShortDescription).HasMaxLength(500);
        builder.Property(x => x.BasePrice).HasPrecision(18, 2);

        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Brand>()
               .WithMany()
               .HasForeignKey(x => x.BrandId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
