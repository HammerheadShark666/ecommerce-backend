using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("ECOMMERCE_Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.IsEmailVerified).HasDefaultValue(false);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.OneTimePasswordSecret).HasMaxLength(250);

        builder.Property(x => x.IsTwoFactorEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasMany(u => u.UserRoles)
               .WithOne(ur => ur.User)
               .HasForeignKey(ur => ur.UserId)
               .OnDelete(DeleteBehavior.Cascade);  

        builder.HasMany(u => u.Addresses)
           .WithOne(ua => ua.User)
           .HasForeignKey(ua => ua.UserId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.Addresses)
               .UsePropertyAccessMode(PropertyAccessMode.Field); 
    }
}
