using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresss");       
        builder.HasKey(x => x.Id);
  
        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(200);

            address.Property(x => x.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(200);

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100);

            address.Property(x => x.County)
                .HasColumnName("County")
                .HasMaxLength(100);

            address.Property(x => x.PostCode)
                .HasColumnName("PostCode")
                .HasMaxLength(20);

            address.Property(x => x.Country)
                .HasColumnName("Country")
                .HasMaxLength(100);
        });
    }
}
