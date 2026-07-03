using ECommerce.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);

        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
