using ECommerce.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.LogoUrl).HasMaxLength(1000);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
