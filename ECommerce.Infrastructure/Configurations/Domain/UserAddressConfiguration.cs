using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresss");       
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Line1).HasMaxLength(200);
            address.Property(x => x.Line2).HasMaxLength(200);
            address.Property(x => x.City).HasMaxLength(100);
            address.Property(x => x.County).HasMaxLength(100);
            address.Property(x => x.Country).HasMaxLength(100);
            address.Property(x => x.PostCode).HasMaxLength(20);            
        }); 
    }
}
