using ECommerce.Domain.Entities.Basket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasMany(x => x.Items)
               .WithOne(x => x.Cart)
               .HasForeignKey(x => x.CartId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
